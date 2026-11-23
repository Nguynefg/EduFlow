using System.Net;
using System.Net.Mail;
using System.Text.Json;
namespace EduFlow.Web.Helpers;
public record PreviewMail(string Id,string To,string Subject,string Body,DateTime CreatedAt);
// Development lưu email mô phỏng trong App_Data, không gửi ra ngoài.
public class EmailSender
{
    private readonly IConfiguration _config;private readonly IWebHostEnvironment _env;private readonly ILogger<EmailSender> _log;
    public EmailSender(IConfiguration config,IWebHostEnvironment env,ILogger<EmailSender> log){_config=config;_env=env;_log=log;}
    public bool IsSimulation=>_env.IsDevelopment() && !string.Equals(_config["Mail:Mode"],"Smtp",StringComparison.OrdinalIgnoreCase);
    private string Folder=>Path.Combine(_env.ContentRootPath,"App_Data","mail");
    public async Task<bool> SendAsync(string to,string subject,string body)
    {
        try
        {
            if(IsSimulation)
            {
                Directory.CreateDirectory(Folder);var id=Guid.NewGuid().ToString("N");
                await File.WriteAllTextAsync(Path.Combine(Folder,id+".json"),JsonSerializer.Serialize(new PreviewMail(id,to,subject,body,DateTime.Now)));return true;
            }
            var host=_config["Smtp:Host"];var from=_config["Smtp:From"];
            if(string.IsNullOrWhiteSpace(host)||string.IsNullOrWhiteSpace(from)){_log.LogWarning("SMTP chưa cấu hình, không gửi email.");return false;}
            using var smtp=new SmtpClient(host,_config.GetValue<int?>("Smtp:Port")??587){EnableSsl=true,Timeout=10000,Credentials=new NetworkCredential(_config["Smtp:Username"],_config["Smtp:Password"])};
            using var message=new MailMessage(from,to,subject,body);await smtp.SendMailAsync(message);return true;
        }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException or SmtpException or FormatException or ArgumentException)
        {_log.LogWarning("Không gửi được email: {Kind}",error.GetType().Name);return false;}
    }
    public async Task<List<PreviewMail>> ListAsync(string? to)
    {
        var result=new List<PreviewMail>();if(!IsSimulation||!Directory.Exists(Folder))return result;
        foreach(var path in Directory.EnumerateFiles(Folder,"*.json").OrderByDescending(File.GetLastWriteTimeUtc))
        {
            var mail=await ReadAsync(Path.GetFileNameWithoutExtension(path));
            if(mail!=null && (to==null || string.Equals(mail.To,to,StringComparison.OrdinalIgnoreCase)))result.Add(mail);
            if(result.Count>=200)break;
        }
        return result;
    }
    public async Task<PreviewMail?> ReadAsync(string id)
    {
        if(!IsSimulation || !Guid.TryParseExact(id,"N",out _))return null;
        var path=Path.Combine(Folder,id+".json");if(!File.Exists(path))return null;
        try{return JsonSerializer.Deserialize<PreviewMail>(await File.ReadAllTextAsync(path));}catch(Exception e) when(e is IOException or JsonException){return null;}
    }
}
