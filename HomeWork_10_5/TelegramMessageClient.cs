using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace HomeWork_10_5
{
    class TelegramMessageClient
    {
        private MainWindow w;
        private TelegramBotClient bot;
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }

        private static string rootPath = "D:\\temp\\";

        public TelegramMessageClient(MainWindow W, string PathToken = @"D:\token")
        {
            this.BotMessageLog = new ObservableCollection<MessageLog>();
            this.w = W;

            bot = new TelegramBotClient(File.ReadAllText(PathToken));

            bot.OnMessage += MessageListener;

            bot.StartReceiving();
        }

        private void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Console.WriteLine("---");
            Debug.WriteLine("+++---");

            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            Debug.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");
            string messageText = "";
           
            

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                Debug.WriteLine(e.Message.Document.FileId);
                Debug.WriteLine(e.Message.Document.FileName);
                Debug.WriteLine(e.Message.Document.FileSize);
                
                messageText = $"загружаю файл - {e.Message.Document.FileName} {e.Message.Document.FileSize}";
                
                this.DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);
                
                SendMessage(messageText, e.Message.Chat.Id);
            }

            if (e.Message.Text == null) return;

            if (e.Message.Text == "/start")
            {
                messageText = $"{e.Message.Chat.FirstName} вас приветсвует секретный бот облако" +
                    "\nотправляйте мне свои документы и файлы я буду их хранить" +
                    "\nСписок доступных комманд:" +
                    "\n/seeFiles" +
                    "\nнапишите Имя файла для скачки";
            }
            else if (e.Message.Text == "/seeFiles")
            {
                messageText = GetDir();
            }else if (File.Exists(rootPath + e.Message.Text))
            {

                UpLoad(e.Message.Text, e.Message.Chat.Id);
                messageText = $"отправлен файл {e.Message.Text}";
            }
            else if (String.IsNullOrEmpty(messageText))
            {
                messageText = e.Message.Text + " - файл отсутсвует в базе данных";

            }
            
            SendMessage(messageText, e.Message.Chat.Id);

           
            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new MessageLog(
                    DateTime.Now.ToLongTimeString(), e.Message.Text, e.Message.Chat.FirstName, e.Message.Chat.Id));
            });
            


        }

        public void SendMessage(string Text, long Id)
        {
            bot.SendTextMessageAsync(Id, Text);

            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new MessageLog(
                    DateTime.Now.ToLongTimeString(), Text, "Секретный бот", 0));
            });


        }


        /// <summary>
        /// выгрузить файл
        /// </summary>
        /// <param name="file"></param>
        public async void UpLoad(string file, long userID)
        {
            using (FileStream stream = File.Open(rootPath + file, FileMode.Open))
            {
                InputOnlineFile iof = new InputOnlineFile(stream);
                iof.FileName = file;
                var send = await bot.SendDocumentAsync(userID, iof, "Вот то что вы просили!");
            }
        }


        /// <summary>
        /// скачать файл
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="path"></param>
        public async void DownLoad(string fileId, string path)
        {
            using (FileStream fs = new FileStream(rootPath + path, FileMode.Create)){
                var file = await bot.GetFileAsync(fileId);
                await bot.DownloadFileAsync(file.FilePath, fs);
            }

            //FileStream fs = new FileStream(rootPath + path, FileMode.Create);
            //fs.Close();
            //fs.Dispose();

        }


        /// <summary>
        /// Получение всех файлов 
        /// </summary>
        /// <param name="path">Путь к каталогу</param>
        /// <param name="trim">Количество отступов</param>
        static string GetDir()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
            StringBuilder text = new StringBuilder();

            foreach (var item in directoryInfo.GetFiles())
            {
                text.Append($"\n{item.Name}  размер файла {item.Length}.");
            }

            return text.ToString();
        }

    }
}
