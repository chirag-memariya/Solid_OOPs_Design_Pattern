using WebApi.Models;
using WebApi.Sockets;

namespace WebApi.Services
{
    public class BaseService
    {
        protected LinuxSocket _commandSocket;
    }
}