using Google.Cloud.Firestore;

namespace Data.Models
{
    [FirestoreData]
    public class Movie
    {
        [FirestoreDocumentId]
        public string UrlName { get; set; }
        [FirestoreProperty]
        public string Name { get; set; }

        // Details
        [FirestoreProperty]
        public int Year { get; set; }
        [FirestoreProperty]
        public int Score { get; set; }
        [FirestoreProperty]
        public int? Runtime { get; set; }
        [FirestoreProperty]
        public string[] Genres { get; set; }
        [FirestoreProperty]
        public string[] Languages { get; set; }

        // Credits
        [FirestoreProperty]
        public string[] DirectorNames { get; set; }
        [FirestoreProperty]
        public string[] DirectorUrlNames { get; set; }
        [FirestoreProperty]
        public string[] WriterNames { get; set; }
        [FirestoreProperty]
        public string[] WriterUrlNames { get; set; }
        [FirestoreProperty]
        public string[] CastNames { get; set; }
        [FirestoreProperty]
        public string[] CastUrlNames { get; set; }
        // TODO add Seen and Starred bools
    }
}
