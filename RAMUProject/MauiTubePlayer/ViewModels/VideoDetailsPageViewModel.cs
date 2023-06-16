namespace MauiTubePlayer.ViewModels;

public partial class VideoDetailsPageViewModel : AppViewModelBase
{
    [ObservableProperty]
    private YoutubeVideoDetail theVideo;

    [ObservableProperty]
    private List<YoutubeVideo> similarVideos;

    [ObservableProperty]
    private Channel theChannel;

    [ObservableProperty]
    private List<Comment> comments;

    public event EventHandler DownloadCompleted;

    [ObservableProperty]
    private string videoSource;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private bool isDownloading = false;

    private IEnumerable<MuxedStreamInfo> streamInfo;
    private IDownloadFileService _fileDownloadService;


    public VideoDetailsPageViewModel(IApiService appApiService, IDownloadFileService fileDownloadService) : base(appApiService)
    {
        this._fileDownloadService = fileDownloadService;

        this.Title = "RAMU YOUTUBE";
    }

    public override async void OnNavigatedTo(object parameters)
    {
        var videoID = (string)parameters;

        SetDataLodingIndicators(true);

        this.LoadingText = "Sacekajte dok pretražujem video detalje...";

        try
        {
            SimilarVideos = new();
            Comments = new();

            //Get Video Details
            TheVideo = await _appApiService.GetVideoDetails(videoID);

            //Get Channel URL and set in the video
            var channelSearchResult = await _appApiService.GetChannels(TheVideo.Snippet.ChannelId);
            TheChannel = channelSearchResult.Items.First();

            //Find Similar Videos
            if (TheVideo.Snippet.Tags is not null)
            {
                var similarVideosSearchResult = await _appApiService.SearchVideos(TheVideo.Snippet.Tags.First(), "");

                SimilarVideos = similarVideosSearchResult.Items;
            }

            //Get Comments
            var commentsSearchResult = await _appApiService.GetComments(videoID);
            Comments = commentsSearchResult.Items;


            //Get Stream URL
            await GetVideoURL();


            //Raise Data Load completed event to the UI
            this.DataLoaded = true;

            //Raise the Event to notify the UI of download completion
            DownloadCompleted?.Invoke(this, new EventArgs());
        }
        catch (InternetConnectionException iex)
        {
            this.IsErrorState = true;
            this.ErrorMessage = "Slaba ili nikakva internet konekcija." + Environment.NewLine + "Molim Vas provjerite Vasu niternet konekciju i pokusajte ponovo.";
            ErrorImage = "nointernet.png";
        }
        catch (Exception ex)
        {
            this.IsErrorState = true;
            this.ErrorMessage = $"Nesto je poslo po zlu. Ako se problem ne rijesi, molim Vas kontaktirajte me na {Constants.EmailAddress} sa error porukom:" + Environment.NewLine + Environment.NewLine + ex.Message;
            ErrorImage = "error.png";
        }
        finally
        {
            SetDataLodingIndicators(false);
        }
    }

    [RelayCommand]
    private async Task UnlikeVideo()
    {
        await PageService.DisplayAlert("Nije implementirano",
            "Unlike funkcija nije implementirana jer fali OAuth Login stavka.",
            "OK");
    }

    [RelayCommand]
    private async Task ShareVideo()
    {
        var textToShare =
            $"Hej, Pronasao sam ovaj cool video, pogledaj ga: https://www.youtube.com/watch?v={TheVideo.Id}";

        //Share
        await Share.RequestAsync(new ShareTextRequest
        {
            Text = textToShare,
            Title = TheVideo.Snippet.Title
        });
    }

    [RelayCommand]
    private async Task DownloadVideo()
    {
        if (IsDownloading)
            return;

        var progressIndicator = new Progress<double>((value) => ProgressValue = value);
        var cts = new CancellationTokenSource();

        try
        {
            IsDownloading = true;

            var urlToDownload = streamInfo.OrderByDescending(video => video.VideoResolution.Area).First().Url;

            //Download the file
            var downloadedFilePath = await _fileDownloadService.DownloadFileAsync(
                urlToDownload,
                TheVideo.Snippet.Title.CleanCacheKey() + ".mp4",
                progressIndicator,
                cts.Token);

            //Save the File
            await Share.RequestAsync(new ShareFileRequest
            {
                File = new ShareFile(downloadedFilePath),
                Title = TheVideo.Snippet.Title
            });
        }
        catch (OperationCanceledException ex)
        {
            //Handle Exception and Cancellation Token here
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [RelayCommand]
    private async Task SubscribeChannel()
    {
        await PageService.DisplayAlert(
            "Nije implementirano",
            "Subscribe opcija nije implementirana jer fali OAuth login stavka",
            "OK");
    }

    [RelayCommand]
    private async Task NavigateToVideoDetailsPage(string videoID)
    {
        await NavigationService.PushAsync(new VideoDetailsPage(videoID));
    }

    private async Task GetVideoURL()
    {
        var youtube = new YoutubeClient();

        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(
            $"https://youtube.com/watch?v={TheVideo.Id}"
        );

        // Get highest quality muxed stream
        streamInfo = streamManifest.GetMuxedStreams();

        var videoPlayerStream = streamInfo.First(video => video.VideoQuality.Label is "240p" or "360p" or "480p");

        VideoSource = videoPlayerStream.Url;
    }

}

