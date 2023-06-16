namespace MauiTubePlayer.ViewModels;

public partial class StartPageViewModel : AppViewModelBase
{
    private string nextToken = string.Empty;
    private string searchTerm = "Fakultet za saobracaj i komunikacije";

    [ObservableProperty]
    private ObservableCollection<YoutubeVideo> youtubeVideos;

    [ObservableProperty]
    private bool isLoadingMore;


    public StartPageViewModel(IApiService appApiService) : base(appApiService)
	{
		this.Title = "RAMU YOUTUBE";
	}

    public override async void OnNavigatedTo(object parameters)
    {
        await Search();
    }

    private async Task Search()
    {
        SetDataLodingIndicators(true);

        LoadingText = "Sacekajte dok pretražujem video detalje...";

        YoutubeVideos = new();

        try
        {
            //Search for videos
            await GetYouTubeVideos();

            this.DataLoaded = true;
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

    private async Task GetYouTubeVideos()
    {
        //Search the videos
        var videoSearchResult = await _appApiService.SearchVideos(searchTerm, nextToken);

        nextToken = videoSearchResult.NextPageToken;

        //Get Channel URLs
        var channelIDs = string.Join(",",
            videoSearchResult.Items.Select(video => video.Snippet.ChannelId).Distinct());

        var channelSearchResult = await _appApiService.GetChannels(channelIDs);

        //Set Channel URL in the Video
        videoSearchResult.Items.ForEach(video =>
            video.Snippet.ChannelImageURL = channelSearchResult.Items.Where(channel =>
                channel.Id == video.Snippet.ChannelId).First().Snippet.Thumbnails.High.Url);

        //Add the Videos for Display
        YoutubeVideos.AddRange(videoSearchResult.Items);
    }

    [RelayCommand]
    private async void OpenSettingPage()
    {
        await PageService.DisplayAlert("Postavke", "Ovaj dio aplikacije nije implementiran", "OK!");
    }

    [RelayCommand]
    private async Task LoadMoreVideos()
    {
        if (IsLoadingMore || string.IsNullOrEmpty(nextToken))
            return;

        IsLoadingMore = true;
        await Task.Delay(2000);
        await GetYouTubeVideos();
        IsLoadingMore = false;
    }

    [RelayCommand]
    private async Task SearchVideos(string searchQuery)
    {
        nextToken = string.Empty;
        searchTerm = searchQuery.Trim();

        await Search();
    }

    [RelayCommand]
    private async Task NavigateToVideoDetailsPage(string videoID)
    {
        await NavigationService.PushAsync(new VideoDetailsPage(videoID));
    }

}

