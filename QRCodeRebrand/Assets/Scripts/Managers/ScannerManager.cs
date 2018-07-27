using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

#if WINDOWS_UWP
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

#endif
using global::ZXing;

using static ZXing.RGBLuminanceSource;

using HoloToolkit.Unity;

/*
 CREDITS : 
    - Mike Taulty from (https://mtaulty.com/2016/12/28/windows-10-uwp-qr-code-scanning-with-zxing-and-hololens/) that helped me much for Zxing functions
    - W0 tutorial from (https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader#select-frame-sources-and-frame-source-groups) for the main code

    All the rest is up to me for adaptation and optimization.
     
*/

public class ScannerManager : Singleton<ScannerManager>
{
    //Global Variable
    #region
    public bool QRFind = false;
    private BarcodeReader barCodeReader;
    public Result result;

    //Event throw by the Scanner
    public delegate void ScannerEvent(string s);
    public event ScannerEvent ScanSucessfull;


    //UWP API Variable. If you have to use one, ensure that your code is encapsuled in a WINDOWS_UWP preprocess condition
#if WINDOWS_UWP
    private SoftwareBitmap backBuffer;
    private bool taskRunning = false;
    MediaFrameSourceGroup selectedGroup = null;
    MediaFrameSourceInfo colorSourceInfo = null;
    MediaCapture mediaCapture;
    MediaFrameReader mediaFrameReader;
    int resW = 0;
    int resH = 0;
    int halfresW = 0;
    int halfresH = 0;
    IBuffer buffer;
    byte[] buffer2;
    //                                      3 | 4
    List<byte[]> subImages; // the order is 1 | 2 in the list (so 1 is the inferior left corner of the pict)
#endif

    void barcodeInit()
    {
        barCodeReader = new BarcodeReader();
        /*barCodeReader.Options.PossibleFormats = new List<BarcodeFormat>();
        barCodeReader.Options.PossibleFormats.Add(BarcodeFormat.CODE_128);
        barCodeReader.Options.PossibleFormats.Add(BarcodeFormat.CODE_39);
        barCodeReader.Options.PossibleFormats.Add(BarcodeFormat.UPC_A);
        barCodeReader.Options.PossibleFormats.Add(BarcodeFormat.UPC_E);*/
        barCodeReader.AutoRotate = true;
        barCodeReader.TryInverted = false;
        barCodeReader.Options.TryHarder = true;


    }
    #endregion


    //Event setup for interaction


    //Code using the UWP API. Is working when compiled and export, but can't compile in Unity without the WINDOW_UWP tag
    #region
#if WINDOWS_UWP
    async void LaunchScanSequence()
    {

        print("Start initialize, preparing cap");

        //MediaGroup Selection
        var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
        foreach (var sourceGroup in frameSourceGroups)
        {
            foreach (var sourceInfo in sourceGroup.SourceInfos)
            {
                if (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview
                    && sourceInfo.SourceKind == MediaFrameSourceKind.Color)
                {
                    colorSourceInfo = sourceInfo;
                    break;
                }
            }
            if (colorSourceInfo != null)
            {
                selectedGroup = sourceGroup;
                break;
            }
        }

        //MediaCapture Init
        var settings = new MediaCaptureInitializationSettings()
        {
            SourceGroup = selectedGroup,
            SharingMode = MediaCaptureSharingMode.SharedReadOnly,
            MemoryPreference = MediaCaptureMemoryPreference.Cpu,
            StreamingCaptureMode = StreamingCaptureMode.Video
        };


        mediaCapture = new MediaCapture();
        try
        {
            await mediaCapture.InitializeAsync(settings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed: " + ex.Message);
            return;
        }

        print("Mid initialize, preparing cap");
        //Find supported format and select one
        var colorFrameSource = mediaCapture.FrameSources[colorSourceInfo.Id];
        IEnumerable<MediaFrameFormat> preferredFormat = colorFrameSource.SupportedFormats;
        MediaFrameFormat x = null;
        int a = 0;
        foreach (MediaFrameFormat r in preferredFormat)
        {
            if (r.VideoFormat.Height * r.VideoFormat.Width > a)
            {
                x = r;
                a = (int)(r.VideoFormat.Height * r.VideoFormat.Width);
                resH = (int)r.VideoFormat.Height;
                resW = (int)r.VideoFormat.Width;
            }
        }
        if (x == null)
        {
            print("error on format");
            // Our desired format is not supported
            return;
        }
        //finalize data structure for the scanner
        print("FrameFormat done, Resolution is " + a + "p, " + resH + "x" + resW);
        /* halfresW = (int)Mathf.Floor(resW / 2);
         halfresH = (int)Mathf.Floor(resH / 2);
         for(int i = 0; i < 4; i++)
         {
             subImages[i] = new byte[((halfresW*4)*(halfresH*4))+2];
             for(int y = 0; y < subImages[i].Length; y++)
             {
                 subImages[i][y] = 0;
             }
         }*/
        buffer = new Windows.Storage.Streams.Buffer((uint)a * 32);
        await colorFrameSource.SetFormatAsync(x);

        print("End initialize, preparing cap");
        //Start capts
        mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(colorFrameSource, MediaEncodingSubtypes.Argb32);
        mediaFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
        await mediaFrameReader.StartAsync();
    }

    private void ColorFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        var mediaFrameReference = sender.TryAcquireLatestFrame();
        var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
        var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

        if (softwareBitmap != null)
        {
            if (softwareBitmap.BitmapPixelFormat != Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8 ||
            softwareBitmap.BitmapAlphaMode != Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            }
            softwareBitmap.CopyToBuffer(buffer);
            buffer2 = buffer.ToArray();

            result = DecodeBufferToQRCode(buffer2, softwareBitmap.PixelWidth, softwareBitmap.PixelHeight, BitmapFormat.BGRA32);
            if (result != null)
            {
                if (ScanSucessfull != null)
                {
                    ScanSucessfull(result.Text);
                }
                print("Call send to zxing, result : " + result.Text);
            }
            else
            {

            }



        }
        mediaFrameReference.Dispose();
    }

    async void CleanerAsync()
    {
        await mediaFrameReader.StopAsync();
        mediaFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
        mediaCapture.Dispose();
        mediaCapture = null;
    }


#endif
    #endregion


    //Code used for Zxing call
    #region
#if WINDOWS_UWP
    public Result DecodeBufferToQRCode(byte[] buffer, int width, int height, BitmapFormat bitmapFormat)
    {
        var zxingResult = barCodeReader.Decode(buffer, width, height, bitmapFormat);
        return (zxingResult);
    }

    public Result DecodeBufferToQRCode(IBuffer buffer, int width, int height, BitmapFormat bitmapFormat)
    {
        return (DecodeBufferToQRCode(buffer.ToArray(), width, height, bitmapFormat));

    }
#endif
    #endregion

    //Code used for picture splitting and decoding. (WIP)
    #region
#if WINDOWS_UWP
    void imageProcess(byte[] image)
    {
        for (int i = 0; i <= image.Length; i++)
        {
            //Have to multiply by 4 every reference due to the fact that the image is in RGBA, mean 8x4 bits, so 4 byte for one p.
            if (i % (resW * 4) < halfresW * 4 && i % (resH * 4) < halfresH * 4) //1
            {

            }
            else if (i % (resW * 4) < halfresW * 4 && i % (resH * 4) > halfresH * 4)//3
            {

            }
            else if (i % (resW * 4) > halfresW * 4 && i % (resH * 4) < halfresH * 4)//2
            {

            }
            else//4
            {

            }
        }

        //Then, process each


    }

    void allocateByteBuffer(byte[] buffer, byte value)
    {

    }
#endif
    #endregion

#if WINDOWS_UWP
    //Start and Stop function for Event callback
    #region
    private void StopScan()
    {
        CleanerAsync();
    }

    private void StartScan()
    {

        LaunchScanSequence();
        print("Scanner has been initialized");

    }
    #endregion
#endif

    // Use this for initialization
    void Start()
    {
        barcodeInit();
        //PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);      
#if !UNITY_EDITOR
        UIDisplayerManager.Instance.StartScanEvent += StartScan;
        UIDisplayerManager.Instance.StopScanEvent += StopScan;
#endif
    }


    // Update is called once per frame
    void Update()
    {


    }


}

