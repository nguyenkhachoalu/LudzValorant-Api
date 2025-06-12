using LudzValorant.Entities;

namespace LudzValorant.Payloads.Request.AccountRequests
{
    public class AccountRequest
    {
        public int ExpireTime { get; set; }
        public string Url { get; set; }
    }
}
