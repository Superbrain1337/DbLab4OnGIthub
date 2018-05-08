using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace Lab4
{
    class Program
    {
        private const string EndpointUrl = @"https://lab2.documents.azure.com:443/";
        private const string PrimaryKey = "u2z5JpiXmx7IPSIt0SswUPwkACpuw9ohl6aEgoVUlda5Z5P1z9dVQlWDa8sYU2VddBCBRr3CmxuI89Ym19gqdw==";
        private DocumentClient client;

        static void Main(string[] args)
        {
            Run();
            MainMenuPrinter();
            /*
             
             Hur det ser ut när man startar appen:
            //////////////////////
            ///Choose an action///
            //////////////////////
            1. Add User
            2. See all users
            *NOTE* Den skall endast ta emot 1, 2 eller ESC
            och när tex 3 trycks så körs alternativ 3.
            Hur det ser ut när man tar val: 1
            /////////////////
            ///Add an user///
            ///////////////// 
            Email:<enter here> 
            *enter is pressed and then pic is enterd*
            Picture:<enter here>
            *the full address from Disk cirectory
            
            *NOTE* ev kolla om adressen som skrivs in facktisk är en address och att det finns något i bild adressen
            
            Hur det ser ut när man tar val: 2
            ///////////////
            ///All users///
            ///////////////
            /  ID  / Email            / Bild godkänd / Bild namn
            / 0001 / Example@Mail.com / Godkänd      /
            / 0002 / Test@Test.co.uk  / Inte godkänd /
            *NOTE* ev byta plats på email o bild godkänd, för o slippa omfromatera så mycket
            ID är bara för att hålla reda på rader, inget annat.
            Ev kunna markera hela rader
            ESC för att gåtillbaka till huvud menyn
             */
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        private static void Run(){
            try
            {
                Program p = new Program();
                p.GetStarted().Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("Welcome. Press any key to continue");
                Console.ReadKey();
            }
        }

        public static void MainMenuPrinter()
        {
            Console.Clear();
            string MenuHeader =
                "/////////////////////////\n" +
                "/// Main Menu         ///\n" +
                "/// Choose an action  ///\n" +
                "/// Press Esc to exit ///\n" +
                "/////////////////////////";

            string Options =
                "///1. Add a new user\n" +
                "///2. Reveiw the database";

            Console.WriteLine($"{MenuHeader}\n{Options}");

            for (; ; )
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Menu1Printer();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Menu2Printer();
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                    default:
                        break;

                }
            }


        }

        public static void Menu1Printer()
        {
            Console.Clear();
            string MenuHeader =
                "/////////////////////////\n" +
                "/// Add user Menu       ///\n" +
                "/// Choose an action    ///\n" +
                "/// Press Esc to return ///\n" +
                "/////////////////////////"; ;
            string Options =
                "/// Press enter\n" +
                "/// Enter the Email address\n" +
                "/// Enter name\n" +
                "/// Enter picture URL\n" +
                "/// Then press Enter\n";
            Console.WriteLine($"{MenuHeader}\n{Options}");
            string email = "";
            string name = "";
            string picURL = "";
            for (; ; )
            {

                //Get email and name
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Enter:
                        email = Console.ReadLine();
                        name = Console.ReadLine();
                        picURL = Console.ReadLine();
                        Console.WriteLine();
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                    default:
                        break;

                }
                if (email != "" && name != "") break;
            }
            try
            {
                Program prg = new Program();
                prg.GetStarted().Wait();
                prg.AddUsersToDB(email, name).Wait();
                prg.AddPictureToDB(picURL).Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message,
                    baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("User was created and DB connected without any errors");
                Console.WriteLine();
            }
        }

        public static void Menu2Printer()
        {
            Console.Clear();
            try
            {
                Program prg = new Program();
                prg.GetStarted().Wait();
                Console.WriteLine("\nUsers");
                prg.GetUsers();
                Console.WriteLine("\nPictures");
                prg.GetPicturesToVerify();
                Console.WriteLine("\nVerified pictures");
                prg.GetVerifiedPictures();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message,
                    baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("DB connected without any errors");
                Console.WriteLine();
            }
            //hämta bös från servern

            //printa ut bös från det som hämtas hem
            //ha en count längst upp som kan visa hur många personer som facktist finns

        }


        private async Task AddUsersToDB(string email, string name)
        {
            //Environment.Exit(0);
            //Do interwebstuff
            User U1 = new User();
            U1.Id = GetRandomIdSequence();
            U1.Email = email;
            U1.Name = name;
            await CreateUserDocumentIfNotExists("UserDB", "UserCollection", U1);
        }

        private async Task AddPictureToDB(string picURL)
        {
            Picture P1 = new Picture();
            P1.Id = GetRandomIdSequence();
            P1.Adress = picURL;
            await CreatePictureDocumentIfNotExists("UserDB", "PictureCollection", P1);
        }


        private async Task GetStarted()
        {
            
            client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = "UserDB" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("UserDB"), new DocumentCollection { Id = "UserCollection" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("UserDB"), new DocumentCollection { Id = "PictureCollection" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("UserDB"), new DocumentCollection { Id = "VerifiedPictureCollection" });

        }
        


        private static string GetRandomIdSequence()
        {
            Random random = new Random();
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(9).ToArray());
        }


        private async Task CreateUserDocumentIfNotExists(string databaseName, string collectionName, User user)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, user.Email));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), user);
                }
                else
                {
                    throw;
                }
            }
        }


        private async Task CreatePictureDocumentIfNotExists(string databaseName, string collectionName, Picture picture)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, picture.Adress));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), picture);
                }
                else
                {
                    throw;
                }
            }
        }

        private void GetPicturesToVerify()
        {
            var SqlList = this.client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri("UserDB", "PictureCollection"), "SELECT q.Adress FROM PictureCollection q");
            Console.WriteLine("Database is working...");
            foreach (object quest in SqlList)
            {
                Console.WriteLine("\n {0}", quest);
            }
        }

        private void GetUsers()
        {
            var SqlList = this.client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri("UserDB", "UserCollection"), "SELECT q.Name FROM UserCollection q");
            Console.WriteLine("Database is working...");
            foreach (object quest in SqlList)
            {
                Console.WriteLine("\n {0}", quest);
            }
        }

        private void GetVerifiedPictures()
        {
            var SqlList = this.client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri("UserDB", "VerifiedPictureCollection"), "SELECT q.Adress FROM VerifiedPictureCollection q");
            Console.WriteLine("Database is working...");
            foreach (object quest in SqlList)
            {
                Console.WriteLine("\n {0}", quest);
            }
        }
    }


    public class User
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        //public string ProfilePicture { get; set; } = "";
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Picture
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        public string Adress { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    
}
