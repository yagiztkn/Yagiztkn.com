using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Runtime.InteropServices;
using Yago.DataAcsess.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;


namespace Yago.WebUI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {

        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var projects = _context.Projects.ToList();
            return View(projects);
        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Önce kullanıcının yazdığı kullanıcı adını (admin) VERİTABANINDA arıyoruz
            var admin = _context.Admins.FirstOrDefault(a => a.Username == username);

            // Eğer sistemde böyle bir kullanıcı varsa işlemlere devam et
            if (admin != null)
            {
                // 2. Formdan girilen şifreyi kıyma makinemize (hashPassword) atıyoruz
                string hashedPassword = hashPassword(password);

                // 3. ASIL KONTROL: Kıyma makinesinden çıkan sonuç, VERİTABANINDAKİ sonuçla aynı mı?
                if (admin.PasswordHash == hashedPassword)
                {
                    // Şifreler eşleşti! Bileti kesip içeri alıyoruz.
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Admin");
                }
            }

            // Eğer kullanıcı adı yoksa veya şifre eşleşmezse hata ver
            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Yago.Core.Entities.Project project)
        {
            try
            {
                project.CreatedDate = System.DateTime.Now;

                if (project.FullDescription == null) project.FullDescription = "";
                if (project.LiveLink == null) project.LiveLink = "";
                if (project.GitHubLink == null) project.GitHubLink = "";

                _context.Projects.Add(project);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                var gercekHata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("Hata: " + gercekHata);
            }
        }

        public IActionResult Delete(int id)
        {
            var project = _context.Projects.Find(id);

            if (project != null)
            {
                _context.Projects.Remove(project);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return RedirectToAction("Index");
            }
            return View(project);
        }

        [HttpPost]
        public IActionResult Update(Yago.Core.Entities.Project updatedProject)
        {
            try
            {
                var existingProject = _context.Projects.Find(updatedProject.ID);
                if (existingProject != null)
                {
                    existingProject.Title = updatedProject.Title;
                    existingProject.ShortDescription = updatedProject.ShortDescription;
                    existingProject.Technologies = updatedProject.Technologies;

                    existingProject.FullDescription = updatedProject.FullDescription ?? "";
                    existingProject.GitHubLink = updatedProject.GitHubLink ?? "";
                    existingProject.LiveLink = updatedProject.LiveLink ?? "";

                    _context.SaveChanges();
                }
                return RedirectToAction("Index");

            }
            catch (System.Exception ex)
            {
                var gercekHata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("Hata: " + gercekHata);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Admin");
        }

        private string hashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool IsPasswordSecure(string Password, out string errorMessage)
        {
            errorMessage = "";
            if(Password.Length < 6)
            {
                errorMessage = "Şifre en az 6 karakter olmalıdır.";
                return false;
            }

            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;

            foreach (char c in Password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                if (!char.IsLetterOrDigit(c)) hasSpecial = true;

            }

            if(!hasUpper) { errorMessage = "Şifre en az bir Büyük    harf içermelidir."; return false; }
            if(!hasLower) { errorMessage = "Şifre en az bir küçük harf içermelidir."; return false; }
            if(!hasDigit) { errorMessage = "Şifre en az bir rakam içermelidir."; return false; }
            if(!hasSpecial) { errorMessage = "Şifre en az bir özel karakter içermelidir."; return false; }

            return true;
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                var admin = _context.Admins.FirstOrDefault();
                if (admin == null)
                {
                    ViewBag.Error = "Sistemde kayıtlı yönetici bulunamadı!";
                    return View();
                }

                // Kıyma makinesini kullanarak eski şifreyi doğruluyoruz
                if (admin.PasswordHash != hashPassword(currentPassword))
                {
                    ViewBag.Error = "Mevcut şifrenizi yanlış girdiniz!";
                    return View();
                }

                // Güvenlik görevlisini kullanarak yeni şifreyi denetliyoruz
                if (!IsPasswordSecure(newPassword, out string errorMessage))
                {
                    ViewBag.Error = errorMessage;
                    return View();
                }

                // Her şey tamamsa yeni şifreyi kıyma makinesinden geçirip SQL'e kaydediyoruz
                admin.PasswordHash = hashPassword(newPassword);
                _context.SaveChanges();

                // Şifre değişti, güvenlik için oturumu kapatıyoruz
                return RedirectToAction("Logout");
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = "Bir hata oluştu: " + (ex.InnerException?.Message ?? ex.Message);
                return View();
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Formdan gelen eski ve yeni şifreyi işler
        [HttpPost]
        public IActionResult ChancePasword(string currentPassword, string newPassword)
        {
            try
            {
                // 1. Veritabanındaki yöneticiyi bul (Sistemde tek admin olduğunu varsayıyoruz)
                var admin = _context.Admins.FirstOrDefault();
                if (admin == null)
                {
                    ViewBag.Error = "Sistemde kayıtlı yönetici bulunamadı!";
                    return View();
                }

                // 2. MEVCUT ŞİFRE KONTROLÜ
                // Kullanıcının girdiği eski şifreyi Hash'leyip, veritabanındaki Hash ile karşılaştırıyoruz
                if (admin.PasswordHash != hashPassword(currentPassword))
                {
                    ViewBag.Error = "Mevcut şifrenizi yanlış girdiniz!";
                    return View();
                }

                // 3. KALİTE KONTROL (Güvenlik Görevlisi)
                // Yeni şifre kurallara uyuyor mu? (Eğer uymazsa out parametresi ile hatayı alıyoruz)
                if (!IsPasswordSecure(newPassword, out string errorMessage))
                {
                    ViewBag.Error = errorMessage; // Şifrede sayı yok, büyük harf yok vs. hatası
                    return View();
                }

                // 4. KIYMA MAKİNESİ (Hashing)
                // Her şey tamamsa yeni şifreyi kriptolayıp veritabanına yazıyoruz
                admin.PasswordHash = hashPassword(newPassword);
                _context.SaveChanges();

                // 5. Şifre değiştiği için güvenlik gereği mevcut oturumu kapatıp yeniden giriş yapmasını istiyoruz
                return RedirectToAction("Logout");
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = "Bir hata oluştu: " + (ex.InnerException?.Message ?? ex.Message);
                return View();
            }
        }




    }
}
