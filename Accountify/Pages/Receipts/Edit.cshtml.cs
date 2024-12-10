using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using System.Globalization;

namespace Accountify.Pages.Receipts
{
    public class EditModel : PageModel
    {
        public ReceiptInfo selectedReceipt = new ReceiptInfo();
        public string? selectedReceiptJson { get; private set; }

        public BuyerInfo buyerInfo = new BuyerInfo();
        public string? buyerInfoJson { get; private set; }

        public List<ArticleInfo> listArticles { get; set; } = new List<ArticleInfo>();
        public string? ListArticlesJson { get; private set; }
        public Dictionary<int, int> quantities { get; set; }
        public string? quantitiesJson { get; private set; }

        String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
        private string defaultCompanyInfo = "Cvetka d.o.o.";

        public BuyerInfo newBuyer = new BuyerInfo();
        public ReceiptInfo newReceipt = new ReceiptInfo();

        /////////////////////////////////////////////// Methods

        public ReceiptInfo obtainSelectedReceipt(int receiptID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM Receipts WHERE ReceiptID = @ReceiptID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ReceiptID", receiptID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReceiptInfo receiptInfo = new ReceiptInfo
                                {
                                    ReceiptID = reader.GetInt32(reader.GetOrdinal("ReceiptID")),
                                    BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID")),
                                    BuyerName = reader.GetString(reader.GetOrdinal("BuyerName")),
                                    BuyerAddress = reader.GetString(reader.GetOrdinal("BuyerAddress")),
                                    ReceiptDate = reader.GetDateTime(reader.GetOrdinal("ReceiptDate")).ToString("dd/MM/yyyy"),
                                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                    CompanyInfo = reader.GetString(reader.GetOrdinal("CompanyInfo"))
                                };
                                receiptInfo.ReceiptArticles = fetchReceiptArticles(receiptInfo.ReceiptID);

                                selectedReceipt = (receiptInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
            return selectedReceipt;
        }
        private void retrieveArticles()
        {
            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
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
        private void retrieveBuyerInfo(int buyerID)
        {
            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
            string sqlQuery = "SELECT * FROM Buyers WHERE BuyerID = @BuyerID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@BuyerID", buyerID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            buyerInfo.BuyerID = reader.GetInt32(reader.GetOrdinal("BuyerID"));
                            buyerInfo.BuyerName = reader.GetString(reader.GetOrdinal("BuyerName"));
                            buyerInfo.BuyerAddress = reader.GetString(reader.GetOrdinal("BuyerAddress"));
                            buyerInfo.BuyerTel = reader.GetString(reader.GetOrdinal("BuyerTel"));
                            buyerInfo.BuyerEmail = reader.GetString(reader.GetOrdinal("BuyerEmail"));
                        }
                    }
                }
            }
        }

        // return <ArticleID, quantity>
        public Dictionary<int, int> findQuantities()
        {
            Dictionary<int, int> quantities = new Dictionary<int, int>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var article in selectedReceipt.ReceiptArticles)
                    {
                        int articleID = article.ArticleID;

                        // Construct the SQL query to fetch the quantity for the current article on the selected receipt
                        string sql = "SELECT Quantity FROM ReceiptItems WHERE ReceiptID = @ReceiptID AND ArticleID = @ArticleID";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ReceiptID", selectedReceipt.ReceiptID);
                            command.Parameters.AddWithValue("@ArticleID", articleID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));
                                    quantities.Add(article.ArticleID, quantity);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in finding quantities: " + ex.ToString());
            }
            return quantities;
        }

        // returns List of Articles included on receipt with idReceipt
        public List<ArticleInfo> fetchReceiptArticles(int idReceipt)
        {
            List<ArticleInfo> boughtArticles = new List<ArticleInfo>();
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT R.*, A.* " +
                        "FROM Receipts AS R " +
                        "INNER JOIN ReceiptItems as RI ON R.ReceiptID = RI.ReceiptID " +
                        "INNER JOIN Articles AS A ON RI.ArticleID = A.ArticleID " +
                        "WHERE R.ReceiptID = @idReceipt";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@idReceipt", idReceipt);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ArticleInfo articleInfo = new ArticleInfo
                                {
                                    ArticleID = reader.GetInt32(reader.GetOrdinal("ArticleID")),
                                    ArticleName = reader.GetString(reader.GetOrdinal("ArticleName")),
                                    ArticlePrice = reader.GetDecimal(reader.GetOrdinal("ArticlePrice")),
                                    ArticleDescription = reader.GetString(reader.GetOrdinal("ArticleDescription"))
                                };

                                boughtArticles.Add(articleInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in fetching ReceiptArticles: " + ex.ToString());
            }
            //foreach (ArticleInfo articleInfo in boughtArticles)
            //    Console.WriteLine(articleInfo.ArticleName);
            return boughtArticles;
        }

        // Return dictionary keys
        public List<ArticleInfo> obtainKeys(Dictionary<ArticleInfo, int> cartInfo)
        {
            var list = new List<ArticleInfo>();
            foreach (var article in cartInfo.Keys)
            {
                list.Add(article);
            }
            return list;
        }

        // returns Dictionary of <article, quantity> from read string (from user inputs)
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

        public void OnGet(int id)
        {
            selectedReceipt = obtainSelectedReceipt(id);
            //TempData["SelectedReceipt"] = selectedReceipt;
            TempData["MyVariable"] = selectedReceipt.ReceiptID.ToString();
            retrieveArticles();
            retrieveBuyerInfo(selectedReceipt.BuyerID);
            quantities = findQuantities();
            buyerInfoJson = System.Text.Json.JsonSerializer.Serialize(buyerInfo);
            ListArticlesJson = System.Text.Json.JsonSerializer.Serialize(listArticles);
            selectedReceiptJson = System.Text.Json.JsonSerializer.Serialize(selectedReceipt);
            quantitiesJson = System.Text.Json.JsonSerializer.Serialize(quantities);
        }
        public IActionResult OnPost()
        {
            retrieveArticles();
            // obtain selectedReceiptID through TempData
            int selectedReceiptID = -1;
            if (TempData.TryGetValue("MyVariable", out var myVariable))
            {
                if (myVariable is string stringValue)
                {
                    if (int.TryParse(stringValue, out int parsedValue))
                    {
                        selectedReceiptID = parsedValue;
                    }
                }
            }
            // making newBuyer
            newBuyer.BuyerName = Request.Form["BuyerName"];
            newBuyer.BuyerAddress = Request.Form["BuyerAddress"];
            newBuyer.BuyerTel = Request.Form["BuyerTel"];
            newBuyer.BuyerEmail = Request.Form["BuyerEmail"];

            // making newReceipt
            newReceipt.BuyerName = Request.Form["BuyerName"];
            newReceipt.BuyerAddress = Request.Form["BuyerAddress"];
            newReceipt.ReceiptDate = Request.Form["ReceiptDate"];
            string totalSum = Request.Form["basketSum"].ToString();
            newReceipt.TotalAmount = decimal.Parse(totalSum.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
            string cartArticles = Request.Form["basketData"].ToString();
            Dictionary<ArticleInfo, int> cartInfo = findReceiptArticles(cartArticles);
            newReceipt.ReceiptArticles = obtainKeys(cartInfo);

            var oldReceiptID = selectedReceiptID;

            // Delete all existing ReceiptItems with oldReceiptID
            DeleteExistingReceiptItems(oldReceiptID);

            // Delete the selected Receipt
            DeleteOldReceipt(oldReceiptID);

            var newBuyerID = insertNewBuyer(newBuyer);
            var newReceiptID = insertNewReceipt(newReceipt, newBuyerID);

            // Insert new ReceiptItems from cartInfo to the selectedReceiptID
            InsertNewReceiptItems(cartInfo, newReceiptID);

            return RedirectToPage("/Index");
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
            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=accountify;Integrated Security=True";

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

        private void DeleteOldReceipt(int receiptID)
        {
            try
            {
                string deleteQuery = "DELETE FROM Receipts WHERE ReceiptID = @ReceiptID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ReceiptID", receiptID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in deleting Receipt: " + ex.ToString());
            }
        }

        private void DeleteExistingReceiptItems(int receiptID)
        {
            try
            {
                string deleteQuery = "DELETE FROM ReceiptItems WHERE ReceiptID = @ReceiptID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ReceiptID", receiptID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in deleting ReceiptItems: " + ex.ToString());
                // Handle any exceptions that may occur during the deletion process
            }
        }

        private void InsertNewReceiptItems(Dictionary<ArticleInfo, int> cartInfo, int newReceiptID)
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
    }
}
