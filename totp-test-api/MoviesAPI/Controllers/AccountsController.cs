using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.DTOs;
using OtpNet;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    //[DisableCors]
    public class AccountsController: ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountsController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        #region identityotp
        [HttpPost("Create")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            var user = new IdentityUser { UserName = model.EmailAddress, Email = model.EmailAddress };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return BuildToken(model.EmailAddress);
            }
            else
            {
                return BadRequest(result.Errors);
            }
            //return StatusCode()
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.EmailAddress, 
                model.Password, isPersistent: false, lockoutOnFailure: true);

            //await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if(result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.EmailAddress);
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                Send("server@server.com", "user@user.com", "Token", token);

                WriteToFile(token);
                return new UserToken { IsTwoFactor = true, Provider = "Email", Email = model.EmailAddress};
                //return BuildToken(model);
            }
            else
            {
                return Unauthorized(new ErrorInfo("Invalid username or password"));
            }

            /*var user = await _userManager.FindByNameAsync(model.EmailAddress);
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            Console.WriteLine(token);*/
        }

        [HttpPost("TwoFA")]
        public async Task<ActionResult<UserToken>> TwoFactorLogin([FromBody] TwoFactorDto twoFactor)
        {
            var user = await _userManager.FindByNameAsync(twoFactor.Email);
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, twoFactor.Provider, twoFactor.Token);
            //var result = await _signInManager.TwoFactorSignInAsync("Email", tfa, false, false);
            
            if(result)
            {
                return BuildToken(twoFactor.Email);
            }
            else
            {
                return Unauthorized(new ErrorInfo("2fa failed"));
            }
        }
        #endregion identityotp

        #region identitytotp
        [HttpPost("loginidentitytotp")]
        public async Task<ActionResult<UserToken>> LoginIdentityTotp([FromBody] UserInfo model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.EmailAddress,
                model.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.EmailAddress);
                var unformatedkey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(unformatedkey))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    unformatedkey = await _userManager.GetAuthenticatorKeyAsync(user);
                    var tokenUri = "otpauth://totp/Testissuer:arnarhaf@test.is?secret=" + unformatedkey + "&issuer=Testissuer";
                    return new UserToken { IsTwoFactor = true, Provider = "App", Email = model.EmailAddress, Token = unformatedkey, TokenUri = tokenUri };
                }
                return new UserToken { IsTwoFactor = true, Provider = "App", Email = model.EmailAddress, IsRegistered = true };
            } else if (result.RequiresTwoFactor)
            {
                return new UserToken { IsTwoFactor = true, Provider = "App", Email = model.EmailAddress, IsRegistered = true };
            }
            else
            {
                return Unauthorized(new ErrorInfo("Invalid username or password"));
            }

        }

        [HttpPost("registeridentitytotp")]
        public async Task<ActionResult<UserToken>> RegisterIdentityTotp([FromBody] TwoFactorDto twoFactor)
        {
            var user = await _userManager.FindByNameAsync(twoFactor.Email);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactor.Token);
            if (!isValid)
            {
                return Unauthorized(new ErrorInfo("Incorrect code"));
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return BuildToken(twoFactor.Email);
        }

        [HttpPost("submitidentitytotp")]
        public async Task<ActionResult<UserToken>> SubmitIdentityTotp([FromBody] TwoFactorDto twoFactor)
        {
            var user = await _userManager.FindByNameAsync(twoFactor.Email);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactor.Token);
            
            if (!isValid)
            {
                return Unauthorized(new ErrorInfo("Incorrect code"));
            }

            return BuildToken(twoFactor.Email);
        }
        #endregion identitytotp

        #region otp
        [HttpPost("LoginOTP")]
        public async Task<ActionResult<UserToken>> LoginOTP([FromBody] UserInfo model)
        {
            var user = _context.UserLogins.FirstOrDefault(x => x.UserName == model.EmailAddress && x.Password == model.Password);
            if (user is null)
            {
                return Unauthorized(new ErrorInfo("Invalid username or password"));
            }
            var otp = generateOTP(6);
            WriteToFile(otp);
            user.Token = otp;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return new UserToken { IsTwoFactor = true, Provider = "Email", Email = model.EmailAddress };
        }

        [HttpPost("TwoFAOTP")]
        public async Task<ActionResult<UserToken>> TwoFactorLoginOTP([FromBody] TwoFactorDto twoFactor)
        {
            var user = _context.UserLogins.FirstOrDefault(x => x.UserName == twoFactor.Email);
            if(user is null || user.Token is null)
            {
                return BadRequest("Not found");
            }
            if (user.Token == twoFactor.Token)
            {
                user.AccessFailedCount=0;
                user.Token = null;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return BuildToken(twoFactor.Email);
            }
            else
            {
                user.AccessFailedCount++;
                if(user.AccessFailedCount >=3)
                {
                    user.AccessFailedCount = 0;
                    user.Token = null;
                }
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Unauthorized("2fa failed");
            }
        }
        #endregion otp

        #region totp
        [HttpPost("logintotp")]
        public async Task<ActionResult<UserToken>> LoginTotp([FromBody] UserInfo model)
        {
            var user = _context.UserLogins.FirstOrDefault(x => x.UserName == model.EmailAddress && x.Password == model.Password);

            if(user is null)
            {
                return Unauthorized(new ErrorInfo("Invalid username or password"));
            }

            if(!user.IsRegisterd)
            {
                var key = KeyGeneration.GenerateRandomKey(20);
                var base32String = Base32Encoding.ToString(key);

                //var otpToken = new OtpToken { token = totp.ComputeTotp(), key = base32String };
                user.Token = base32String;

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var tokenUri = "otpauth://totp/Testissuer:arnarhaf@test.is?secret=" + base32String + "&issuer=Testissuer";
                return new UserToken { IsTwoFactor = true, Provider = "App", Email = model.EmailAddress, Token=base32String, TokenUri=tokenUri };
            }

            return new UserToken { IsTwoFactor = true, Provider = "App", Email = model.EmailAddress, IsRegistered =  user.IsRegisterd};
        }

        [HttpPost("registertotp")]
        public async Task<ActionResult<UserToken>> RegisterTotp([FromBody] TwoFactorDto twoFactor)
        {
            var user =  _context.UserLogins.FirstOrDefault(x => x.UserName == twoFactor.Email);
            if (user is null)
            {
                return BadRequest("Invalid login attempt");
            }
            if (user.Token is null)
            {
                return BadRequest("Invalid login attempt");
            }
            if(user.IsRegisterd)
            {
                return BadRequest("User already registerd");
            }

            var base32Bytes = Base32Encoding.ToBytes(user.Token);
            var totp = new Totp(base32Bytes);
            var window = new VerificationWindow(previous: 1, future: 0);
            long timeWindowUsed;
            if(!totp.VerifyTotp(twoFactor.Token, out timeWindowUsed, window))
            {
                return Unauthorized(new ErrorInfo("Incorrect code"));
            }

            user.IsRegisterd = true;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return BuildToken(twoFactor.Email);
        }

        [HttpPost("submittotp")]
        public async Task<ActionResult<UserToken>> SubmitTotp([FromBody] TwoFactorDto twoFactor)
        {
            var user = _context.UserLogins.FirstOrDefault(x => x.UserName == twoFactor.Email);
            if (user is null)
            {
                return BadRequest("Invalid login attempt");
            }
            if (!user.IsRegisterd)
            {
                return BadRequest("User is not registerd");
            }

            var base32Bytes = Base32Encoding.ToBytes(user.Token);
            var totp = new Totp(base32Bytes);
            var window = new VerificationWindow(previous: 1, future: 0);
            long timeWindowUsed;
            if (!totp.VerifyTotp(twoFactor.Token, out timeWindowUsed, window))
            {
                return Unauthorized(new ErrorInfo("Incorrect code"));
            }

            return BuildToken(twoFactor.Email);
        }
        #endregion totp

        [HttpDelete("clearalltests")]
        public async Task<ActionResult> ClearAllTests()
        {
            var users = _context.Users.Where(x => x.Email != null);
            foreach(var user in users)
            {
                user.TwoFactorEnabled = false;
            }
            var tokens = _context.UserTokens.Where(x => x.UserId != null);
            foreach(var token in tokens)
            {
                _context.UserTokens.Remove(token);
            }
            var userLogins = _context.UserLogins.Where(x => x.Id >= 0);
            foreach(var user in userLogins)
            {
                user.Token = null;
                user.IsRegisterd = false;
            }
            _context.SaveChanges();
            return Ok("Cleared");
        }

        /*[HttpGet("totp")]
        public ActionResult Totp()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            var base32String = Base32Encoding.ToString(key);
            var base32Bytes = Base32Encoding.ToBytes(base32String);
            var totp = new Totp(base32Bytes);

            var otpToken = new OtpToken { token = totp.ComputeTotp(), key = base32String };

            var tokenUri = "otpauth://totp/Testissuer:arnarhaf@test.is?secret=" + base32String + "&issuer=Testissuer";

            return Ok(tokenUri);
        }

        [HttpGet("totpcode")]
        public ActionResult TotpCode()
        {
            var base32String = "WASBQ7QDIUM3GAOJVG4HCI7PUFX3GFRR";
            var base32Bytes = Base32Encoding.ToBytes(base32String);
            var totp = new Totp(base32Bytes);

            var otpToken = new OtpToken { token = totp.ComputeTotp(), key = base32String };
            return Ok(otpToken);
        }*/


        //private UserToken BuildToken(UserInfo userInfo)
        private UserToken BuildToken(string email)
        {
            var claims = new List<Claim>()
            {
                //new Claim(ClaimTypes.Name, userInfo.EmailAddress),
                //new Claim(ClaimTypes.Email, userInfo.EmailAddress),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim("myKey", "whatever value i want")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        private void WriteToFile(string message)
        {
            //var p = Environment.CurrentDirectory;
            //var path = "C:\\Users\\spyde\\source\\repos\\si-identity-test-api\\MoviesAPI\\wwwroot\\token1.txt";
            var path = Environment.CurrentDirectory + "\\wwwroot\\token1.txt";
            using (StreamWriter writer = new StreamWriter(path, append: false))
            {
                writer.WriteLine(message);
            }
        }

        private static string generateOTP(int len)
        {
            var str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var n = str.Length;
            var rd = new Random();

            var otp = "";

            for (int i = 1; i <= len; i++)
            {
                otp += str[rd.Next(0, n)];
            }
            return otp;
        }

        private static string generateHOTP(string key)
        {
            var SecretKey = "5LCI5PJEAWGMR3OTJIDPQUTIUWNTAQNC";
            var base32Bytes = Base32Encoding.ToBytes(SecretKey);
            var hotp = new Hotp(base32Bytes);
            var timeStep = CalculateTimeStepFromTimestamp(DateTime.UtcNow);
            var token = hotp.ComputeHOTP(timeStep);
            //Console.WriteLine(token);
            return token;
        }

        const long unixEpochTicks = 621355968000000000L;
        const long ticksToSeconds = 10000000L;
        const int step = 30;
        static long CalculateTimeStepFromTimestamp(DateTime timestamp)
        {
            var unixTimestamp = (timestamp.Ticks - unixEpochTicks) / ticksToSeconds;
            var window = unixTimestamp / (long)step;
            return window;
        }

        public void Send(string from, string to, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Text) { Text = message };

            using var smtp = new SmtpClient();
            // Testing email with Papercut SMTP
            smtp.Connect("127.0.0.1", 25, SecureSocketOptions.None);
            //smtp.Authenticate("", "");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
