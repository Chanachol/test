using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SentEmailsModel : PageModel
{
    private readonly ILogger<SentEmailsModel> _logger;

    public SentEmailsModel(ILogger<SentEmailsModel> logger)
    {
        _logger = logger;
    }

    public List<EmailInfo> Emails { get; set; } = new List<EmailInfo>();

    public async Task OnGetAsync()
    {
        await LoadSentEmailsAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string emailId)
    {
        string connectionString = "Server=tcp:idkj.database.windows.net,1433;Initial Catalog=idkjFinals;Persist Security Info=False;User ID=idkj123;Password=idkj@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM Emails WHERE EmailId = @EmailId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EmailId", emailId);
                    await command.ExecuteNonQueryAsync();
                }
            }

            _logger.LogInformation($"Sent email with ID {emailId} deleted successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError($"Database error while deleting sent email: {sqlEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
        }

        return RedirectToPage();
    }

    private async Task LoadSentEmailsAsync()
    {
        string connectionString = "Server=tcp:idkj.database.windows.net,1433;Initial Catalog=idkjFinals;Persist Security Info=False;User ID=idkj123;Password=idkj@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        Emails.Clear();

        try
        {
    
            string username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("No user logged in. Sent emails cannot be fetched.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT EmailId, Subject, Body, Timestamp, IsRead, Sender, Receiver, AttachmentPath
                    FROM Emails
                    WHERE Sender = @username
                    ORDER BY Timestamp DESC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            EmailInfo emailInfo = new EmailInfo
                            {
                                EmailID = reader["EmailId"].ToString(),
                                EmailSubject = reader["Subject"].ToString(),
                                EmailMessage = reader["Body"].ToString(),
                                EmailDate = Convert.ToDateTime(reader["Timestamp"]).ToString("dd MMM yyyy HH:mm:ss"),
                                EmailSender = reader["Sender"].ToString(),
                                EmailReceiver = reader["Receiver"].ToString(),
                                AttachmentPath = reader["AttachmentPath"] == DBNull.Value ? null : reader["AttachmentPath"].ToString()
                            };

                            Emails.Add(emailInfo);
                        }
                    }
                }
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError($"Database error while fetching sent emails: {sqlEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
        }
    }
    public class EmailInfo
    {
        public string EmailID { get; set; }
        public string EmailSubject { get; set; }
        public string EmailMessage { get; set; }
        public string EmailDate { get; set; }
        public string IsRead { get; set; }
        public string EmailSender { get; set; }
        public string EmailReceiver { get; set; }
        public string AttachmentPath { get; set; }
    }
}

