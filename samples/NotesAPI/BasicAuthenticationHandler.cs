using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace NotesAPI
{
    public class BasicAuthenticationHandler : BasicAuthenticationEvents
    {
        public const string ClaimType_MemberId = "http://schemas.madeoftech.com/ws/2020/08/identity/claims/memberid";
        public override async Task ValidatePrincipalAsync(ValidatePrincipalContext context)
        {
            if (String.IsNullOrEmpty(context.UserName)) return;

            using (var sqliteConnection = new SQLiteConnection("Data Source=notesdb.sqlite"))
            {
                await sqliteConnection.OpenAsync();
                var members = await sqliteConnection.QueryAsync("SELECT * FROM member WHERE login=@login AND plainpassword=@password", new { login = context.UserName, password = context.Password });
                await sqliteConnection.CloseAsync();

                if (members.Count() == 1 && members.First().login == context.UserName)
                {
                    var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, context.UserName, context.Options.ClaimsIssuer),
                                new Claim(ClaimTypes.Email, members.First().email, context.Options.ClaimsIssuer),
                                new Claim(ClaimTypes.GivenName, members.First().givenname, context.Options.ClaimsIssuer),
                                new Claim(ClaimType_MemberId, members.First().id.ToString(), context.Options.ClaimsIssuer),
                            };

                    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Principal = principal;
                }
                //else
                //{
                //    // optional with following default.
                //    // context.AuthenticationFailMessage = "Authentication failed."; 
                //}

                //return Task.CompletedTask;
            }
        }
    }
}
