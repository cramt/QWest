using SmtpServer;
using SmtpServer.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace EmailServer {
    internal class Authenticator : IUserAuthenticatorFactory, IUserAuthenticator {
        public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken) {
            return Task.FromResult(user == InternalProvider.Username && password == InternalProvider.Password);
        }

        public IUserAuthenticator CreateInstance(ISessionContext context) {
            return new Authenticator();
        }
    }
}