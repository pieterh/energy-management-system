using System;
namespace EMS.BlazorWasm.Shared
{
	public class LoginFormModel
	{
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        public LoginFormModel()
		{
		}
	}
}

