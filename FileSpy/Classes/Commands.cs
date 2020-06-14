namespace FileSpy.Classes
{
    static class Commands
    { 
        public static int Disconnect { get; private set; } = -1;
        public static int Login { get; private set; } = 1;
        public static int AcceptLogin { get; private set; } = 2;

        public static int GetList { get; private set; } = 3;
        public static int List { get; private set; } = 4;
        public static int AddUser { get; private set; } = 5;
        public static int RemoveUser { get; private set; } = 6;

        public static int Ping { get; private set; } = 666;

        public static int RsaKey { get; private set; } = 7;
        public static int AesKey { get; private set; } = 8;

        public static int KeyPass { get; private set; } = 9;
        public static int ChangeStatus { get; private set; } = 10;

        public static int ChangeName { get; private set; } = 11;

        public static int RFileSend { get; private set; } = 12;
        public static int AcceptFile { get; private set; } = 13;
        public static int CancelFile { get; private set; } = 14;
        public static int FileInfo { get; private set; } = 15;
        public static int FileData { get; private set; } = 16;
        public static int FileOK { get; private set; } = 17;
        public static int FileDone { get; private set; } = 34;

        public static int RVideoModule { get; private set; } = 18;
        public static int HVideoModule { get; private set; } = 19;
        public static int VideoData { get; private set; } = 20;
        public static int VideoPulsar { get; private set; } = 21;
        public static int VideoClose { get; private set; } = 22;
        public static int VideoDenied { get; private set; } = 23;

        public static int SetVideo { get; private set; } = 24;
        public static int SetMaxFps { get; private set; } = 25;
        public static int SetSize { get; private set; } = 26;
        public static int SetQuality { get; private set; } = 27;
        public static int SetMicro { get; private set; } = 28;
        public static int SetAudio { get; private set; } = 29;

        public static int MicroData { get; private set; } = 30;

        public static int GetInfo { get; private set; } = 31;
        public static int Info { get; private set; } = 32;

        public static int SetCursor { get; private set; } = 33;

        public static int RFileModule { get; private set; } = 35;
        public static int HFileModule { get; private set; } = 36;
        public static int FileMAccepted { get; private set; } = 37;
        public static int FileMDenied { get; private set; } = 38;
        public static int CD { get; private set; } = 39;
        public static int Dirs { get; private set; } = 40;
        public static int FilePulsar { get; private set; } = 41;
        public static int Run { get; private set; } = 42;
        public static int RunWith { get; private set; } = 43;
        public static int Delete { get; private set; } = 44;
        public static int StartDownload { get; private set; } = 45;
        public static int StopDownload { get; private set; } = 46;
        public static int FileDData { get; private set; } = 47;
        public static int FileUData { get; private set; } = 48;
        public static int GetDirInfo { get; private set; } = 49;
        public static int DirInfo { get; private set; } = 50;
        public static int FileUError { get; private set; } = 51;

        public static int SetLoop { get; private set; } = 52;
        public static int LoopInfo { get; private set; } = 53;
        public static int LoopData { get; private set; } = 54;
    }
}
