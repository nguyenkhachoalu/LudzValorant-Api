using LudzValorant.Entities;
using LudzValorant.Payloads.ResponseModels.DataUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Payloads.Mappers
{
    public class UserConverter
    {
        public DataResponseUser EntitytoDTO(User user)
        {
            return new DataResponseUser()
            {
                Id = user.Id,
                Username = user.Username,
                Avatar = user.Avatar,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
            };
        }
    }

}
