using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

public class ReadMailModel : PageModel
{
    public EmailInfo Email { get; set; }

    public async Task<IActionResult> OnGetAsync(int emailId)
    {
        string connectionString = "Server=tcp:idkj.database.windows.net,1433;Initial Catalog=idkjFinals;Persist Security Info=False;User ID=idkj123;Password=idkj@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        if (emailId <= 0)
        {
            return BadRequest("Invalid email ID.");
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                SELECT EmailId, Subject, Body, Timestamp, Sender, Receiver, AttachmentPath
                FROM Emails
                WHERE EmailId = @EmailId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EmailId", emailId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Email = new EmailInfo
                            {
                                EmailID = reader["EmailId"].ToString(),
                                EmailSubject = reader["Subject"].ToString(),
                                EmailMessage = reader["Body"].ToString(),
                                EmailDate = Convert.ToDateTime(reader["Timestamp"]).ToString("dd MMM yyyy HH:mm:ss"),
                                EmailSender = reader["Sender"].ToString(),
                                EmailReceiver = reader["Receiver"].ToString(),
                                AttachmentPath = reader["AttachmentPath"]?.ToString()
                            };
                        }
                        else
                        {
                            return NotFound("Email not found.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return Page();
    }

    public IActionResult OnPostDownload(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        var fileName = Path.GetFileName(filePath);
        var contentType = "application/octet-stream";

        return PhysicalFile(filePath, contentType, fileName);
    }
}
