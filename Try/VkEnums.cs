using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Try
{
    [Flags]
    enum VkontakteScopeList
    {
        Notify = 1,
        Friends = 2,
        Photos = 4,
        Audio = 8,
        Video = 16,
        Offers = 32,
        Questions = 64,
        Pages = 128,
        Link = 256,
        Status = 1024,
        Notes = 2048,
        Messages = 4096,
        Wall = 8192,
        Ads = 32768,
        Offline = 65536,
        Docs = 131072,
        Groups = 262144,
        Notifications = 524288,
        Stats = 1048576,
        Email = 4194304,
        Market = 134217728
    }

    enum SortingOptions
    {
        Date = 0,
        Duration = 1,
        Popularity = 2
    }

    public enum SongGenres
    {
        Rock = 1,
        Pop = 2,
        RapAndHipHop = 3,
        EasyListening = 4,
        DanceAndHouse = 5,
        Instrumental = 6,
        Metal = 7,
        Dubstep = 8,
        DrumAndBass = 10,
        Trance = 11,
        Chanson = 12,
        Ethnic = 13,
        AcousticAndVocal = 14,
        Reggae = 15,
        Classical = 16,
        IndiePop = 17,
        Other = 18,
        Speech = 19,
        Alternative = 21,
        ElectropopAndDisco = 22,
        JazzAndBlues = 1001,
    }

    public enum VkErrors
    {
        UnknownError=1,
        ApplicationIsSomething,
        UnknownMethod,
        WrongSignature,
        CanntotAutoriseUser,
        ToManyRequests,
        NoRights,
        WrongRequest,
        ToManySameRequests,
        InternalServerError,
        TestSomething, //idk the translation
        CaptchRequired=14,
        AccessDenied,
        HttpsRequired,
        ValidationRequired,
        ForbiddenForNonStandaloneApplications = 20,
        OnlyForStandaloneApplications,
        //Some other errors,but i am to lazy to include useless ones
        AlbumAccessDenied=200,
        AudioAccessDenied=201,
        ClubAccessDenied=203,
        AlbumIsFull=300,
     }

    public enum TabSource
    {
        VkEdiatable,
        VkNonEditable,
        VkPlaylist,
        LocalPlaylist,
        Customtab
    }
}
