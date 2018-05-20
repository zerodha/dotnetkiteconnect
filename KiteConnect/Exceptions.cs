using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace KiteConnect
{
    public class KiteException : Exception 
    {
        HttpStatusCode Status;
        public KiteException(string Message, HttpStatusCode HttpStatus) : base(Message) { Status = HttpStatus; }
    }

    public class GeneralException : KiteException {
        public GeneralException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.InternalServerError) : base(Message, HttpStatus) { }
    }

    public class TokenException : KiteException {
        public TokenException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.Forbidden) : base(Message, HttpStatus) { }
    }


    public class PermissionException : KiteException {
        public PermissionException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.Forbidden) : base(Message, HttpStatus) { }
    }

    public class OrderException : KiteException
    {
        public OrderException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.BadRequest) : base(Message, HttpStatus) { }
    }

    public class InputException : KiteException
    {
        public InputException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.BadRequest) : base(Message, HttpStatus) { }
    }

    public class DataException : KiteException
    {
        public DataException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.BadGateway) : base(Message, HttpStatus) { }
    }

    public class NetworkException : KiteException
    {
        public NetworkException(string Message, HttpStatusCode HttpStatus = HttpStatusCode.ServiceUnavailable) : base(Message, HttpStatus) { }
    }

}
