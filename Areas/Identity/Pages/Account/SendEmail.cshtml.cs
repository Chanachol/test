using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

public class SendEmailModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Recipient email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string EmailReceiver { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Subject is required.")]
    public string EmailSubject { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Message is required.")]
    public string EmailMessage { get; set; }

    [BindProperty]
    public IFormFile? EmailAttachment { get; set; }

    public string StatusMessage { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string senderEmail = User.Identity?.Name;

        
        if (string.Equals(senderEmail, EmailReceiver, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Error: You cannot send an email to yourself.";
            return Page();
        }

        string connectionString = "Server=tcp:idkj.database.windows.net,1433;Initial Catalog=idkjFinals;Persist Security Info=False;User ID=idkj123;Password=idkj@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                
                string checkRecipientSql = "SELECT COUNT(*) FROM AspNetUsers WHERE LOWER(Email) = LOWER(@EmailReceiver)";
                using (SqlCommand checkCommand = new SqlCommand(checkRecipientSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@EmailReceiver", EmailReceiver);

                    int recipientExists = (int)await checkCommand.ExecuteScalarAsync();
                    if (recipientExists == 0)
                    {
                        StatusMessage = "Error: The recipient email does not exist.";
                        return Page();
                    }
                }

                
                string filePath = null;
                if (EmailAttachment != null && EmailAttachment.Length > 0)
                {
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadFolder);
                    filePath = Path.Combine(uploadFolder, EmailAttachment.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await EmailAttachment.CopyToAsync(stream);
                    }
                }

                
                string insertEmailSql = @"
            INSERT INTO Emails (Subject, Body, Timestamp, IsRead, Sender, Receiver, AttachmentPath)
            VALUES (@Subject, @Body, @Timestamp, @IsRead, @Sender, @Receiver, @AttachmentPath)";

                using (SqlCommand insertCommand = new SqlCommand(insertEmailSql, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Subject", EmailSubject);
                    insertCommand.Parameters.AddWithValue("@Body", EmailMessage);
                    insertCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    insertCommand.Parameters.AddWithValue("@IsRead", 0);
                    insertCommand.Parameters.AddWithValue("@Sender", senderEmail ?? "Anonymous");
                    insertCommand.Parameters.AddWithValue("@Receiver", EmailReceiver);

                    if (string.IsNullOrEmpty(filePath))
                    {
                        insertCommand.Parameters.AddWithValue("@AttachmentPath", DBNull.Value);
                    }
                    else
                    {
                        insertCommand.Parameters.AddWithValue("@AttachmentPath", filePath);
                    }

                    await insertCommand.ExecuteNonQueryAsync();
                }
            }

            StatusMessage = "Your mail is successfully sent!";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            return Page();
        }
    }
}