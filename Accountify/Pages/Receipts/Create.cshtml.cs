using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using System.Globalization;
using System.Dynamic;

namespace Accountify.Pages.Receipts
{

    public class CreateModel : PageModel
    {
        public List<ArticleInfo> listArticles { get; set; } = new List<ArticleInfo>();

        public BuyerInfo newBuyer = new BuyerInfo();
        public ReceiptInfo newReceipt = new ReceiptInfo();
        public string? ListArticlesJson { get; private set; }
        string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
        private string defaultCompanyInfo = "Cvetka d.o.o.";

        ///////////////////////////////////////////////////// Methods

        public void showReceiptInfo(List<ArticleInfo> receiptArticles)
        {
            Console.WriteLine("Name " + newBuyer.BuyerName);
            Console.WriteLine("Address " + newBuyer.BuyerAddress);
            Console.WriteLine("Telephone " + newBuyer.BuyerTel);
            Console.WriteLine("Email " + newBuyer.BuyerEmail);
            Console.WriteLine("Date " + newReceipt.ReceiptDate);
            Console.WriteLine("Sum " + newReceipt.TotalAmount);
            Console.WriteLine("Basket: ");
            foreach (var article in receiptArticles)
            {
                Console.WriteLine("--Id: " + article.ArticleID + "   Name: " + article.ArticleName);
            }
        }
        public List<ArticleInfo> obtainKeys(Dictionary<ArticleInfo, int> cartInfo)
        {
            var list = new List<ArticleInfo>();
            foreach (var article in cartInfo.Keys)
            {
                list.Add(article);
            }
            return list;
        }
        public Dictionary<ArticleInfo, int> findReceiptArticles(string cartArticles)
        {
            var cartArticlesList = cartArticles.Split(',');
            Dictionary<ArticleInfo, int> articleInfo = new Dictionary<ArticleInfo, int>();

            foreach (var cartArticle in cartArticlesList)
            {
                var parts = cartArticle.Split('-');

                if (parts.Length == 2 && int.TryParse(parts[0], out int articleID) && int.TryParse(parts[1], out int quantity))
                {
                    foreach (var article in listArticles)
                    {
                        if (articleID == article.ArticleID)
                        {
                            articleInfo[article] = quantity;
                        }
                    }
                }
                else
                {
                    // Handle invalid cartArticle format or parsing errors
                    Console.WriteLine("Invalid cartArticle format: " + cartArticle);
                }
            }
            return articleInfo;
        }
        private void retrieveArticles()
        {
            string sqlQuery = "SELECT * FROM Articles";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ArticleInfo article = new ArticleInfo
                            {
                                ArticleID = reader.GetInt32(reader.GetOrdinal("ArticleID")),
                                ArticleName = reader.GetString(reader.GetOrdinal("ArticleName")),
                                ArticlePrice = reader.GetDecimal(reader.GetOrdinal("ArticlePrice")),
                                ArticleDescription = reader.GetString(reader.GetOrdinal("ArticleDescription"))
                            };
                            listArticles.Add(article);
                        }
                    }
                }
            }
        }
        private int insertNewBuyer(BuyerInfo newBuyer)
        {
            int generatedBuyerID = -1;

            string checkIfExistsQuery = "SELECT BuyerID FROM Buyers WHERE BuyerName = @BuyerName AND BuyerAddress = @BuyerAddress";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand checkIfExistsCommand = new SqlCommand(checkIfExistsQuery, connection))
                {
                    checkIfExistsCommand.Parameters.AddWithValue("@BuyerName", newBuyer.BuyerName);
                    checkIfExistsCommand.Parameters.AddWithValue("@BuyerAddress", newBuyer.BuyerAddress);

                    object buyerID = checkIfExistsCommand.ExecuteScalar();

                    if (buyerID != null)
                    {

                        generatedBuyerID = (int)buyerID;
                    }
                }

                if (generatedBuyerID == -1)
                {
                    string insertBuyerQuery = "INSERT INTO Buyers (BuyerName, BuyerAddress, BuyerTel, BuyerEmail) " +
                                              "VALUES (@BuyerName, @BuyerAddress, @BuyerTel, @BuyerEmail); " +
                                              "SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    using (SqlCommand insertBuyerCommand = new SqlCommand(insertBuyerQuery, connection))
                    {
                        insertBuyerCommand.Parameters.AddWithValue("@BuyerName", newBuyer.BuyerName);
                        insertBuyerCommand.Parameters.AddWithValue("@BuyerAddress", newBuyer.BuyerAddress);
                        insertBuyerCommand.Parameters.AddWithValue("@BuyerTel", newBuyer.BuyerTel);
                        insertBuyerCommand.Parameters.AddWithValue("@BuyerEmail", newBuyer.BuyerEmail);

                        generatedBuyerID = (int)insertBuyerCommand.ExecuteScalar();
                    }
                }
            }

            return generatedBuyerID;
        }
        private int insertNewReceipt(ReceiptInfo newReceipt, int buyerID)
        {
            int generatedReceiptID = 0;

            string insertReceiptQuery = "INSERT INTO Receipts (BuyerID, BuyerName, BuyerAddress, ReceiptDate, TotalAmount, CompanyInfo) " +
                                        "VALUES (@BuyerID, @BuyerName, @BuyerAddress, @ReceiptDate, @TotalAmount, @CompanyInfo); " +
                                        "SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand insertReceiptCommand = new SqlCommand(insertReceiptQuery, connection))
                {
                    insertReceiptCommand.Parameters.AddWithValue("@BuyerID", buyerID);
                    insertReceiptCommand.Parameters.AddWithValue("@BuyerName", newReceipt.BuyerName);
                    insertReceiptCommand.Parameters.AddWithValue("@BuyerAddress", newReceipt.BuyerAddress);
                    insertReceiptCommand.Parameters.AddWithValue("@ReceiptDate", newReceipt.ReceiptDate);
                    insertReceiptCommand.Parameters.AddWithValue("@TotalAmount", newReceipt.TotalAmount);
                    insertReceiptCommand.Parameters.AddWithValue("@CompanyInfo", defaultCompanyInfo);

                    generatedReceiptID = (int)insertReceiptCommand.ExecuteScalar();
                }
            }

            return generatedReceiptID; // Return the generated receipt ID
        }
        private void insertNewReceiptItems(Dictionary<ArticleInfo, int> cartInfo, int newReceiptID)
        {
            string sqlQuery = "INSERT INTO ReceiptItems (ReceiptID, ArticleID, Quantity) " +
                              "VALUES (@ReceiptID, @ArticleID, @Quantity)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var cartItem in cartInfo)
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ReceiptID", newReceiptID);
                        command.Parameters.AddWithValue("@ArticleID", cartItem.Key.ArticleID);
                        command.Parameters.AddWithValue("@Quantity", cartItem.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public IActionResult OnGet()
        {
            // Call the method to retrieve articles from DB
            retrieveArticles();
            ListArticlesJson = System.Text.Json.JsonSerializer.Serialize(listArticles);
            return Page();
        }

        public IActionResult OnPost()
        {
            retrieveArticles();
            newBuyer.BuyerName = Request.Form["BuyerName"];
            newBuyer.BuyerAddress = Request.Form["BuyerAddress"];
            newBuyer.BuyerTel = Request.Form["BuyerTel"];
            newBuyer.BuyerEmail = Request.Form["BuyerEmail"];

            newReceipt.BuyerName = Request.Form["BuyerName"];
            newReceipt.BuyerAddress = Request.Form["BuyerAddress"];
            newReceipt.ReceiptDate = Request.Form["ReceiptDate"];

            string totalSum = Request.Form["basketSum"].ToString();
            newReceipt.TotalAmount = decimal.Parse(totalSum.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

            string cartArticles = Request.Form["basketData"].ToString();
            Dictionary<ArticleInfo, int> cartInfo = findReceiptArticles(cartArticles);
            newReceipt.ReceiptArticles = obtainKeys(cartInfo);

            showReceiptInfo(newReceipt.ReceiptArticles);
            var newBuyerID = insertNewBuyer(newBuyer);
            var newReceiptID = insertNewReceipt(newReceipt, newBuyerID);
            insertNewReceiptItems(cartInfo, newReceiptID);
            return RedirectToPage("/Index");
        }
    }
}
