using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HomeModel : PageModel
{
    public List<EmailInfo> Emails { get; set; } = new List<EmailInfo>();

    public async Task OnGetAsync()
    {
        await LoadEmailsAsync();
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
        }
        catch
        {
            return StatusCode(500, "Internal server error.");
        }

        return RedirectToPage();
    }

    private async Task LoadEmailsAsync()
    {
        string connectionString = "Server=tcp:idkj.database.windows.net,1433;Initial Catalog=idkjFinals;Persist Security Info=False;User ID=idkj123;Password=idkj@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

      
        Emails.Clear();

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

            
                string username = User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                {
                 
                    return;
                }

                string sql = @"
                    SELECT EmailId, Subject, Body, Timestamp, Sender, Receiver, AttachmentPath 
                    FROM Emails 
                    WHERE Receiver = @username 
                    ORDER BY Timestamp DESC";


                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Emails.Add(new EmailInfo
                            {
                                EmailID = reader["EmailId"].ToString(),
                                EmailSubject = reader["Subject"].ToString(),
                                EmailMessage = reader["Body"].ToString(),
                                EmailDate = Convert.ToDateTime(reader["Timestamp"]).ToString("dd MMM yyyy HH:mm:ss"),
                                EmailSender = reader["Sender"].ToString(),
                                EmailReceiver = reader["Receiver"].ToString(),
                                AttachmentPath = reader["AttachmentPath"] == DBNull.Value ? null : reader["AttachmentPath"].ToString()
                            });

                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
        }
    }
}
//dfgdgdfgasdasd
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
