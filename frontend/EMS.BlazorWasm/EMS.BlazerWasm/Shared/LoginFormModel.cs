using System;
namespace EMS.BlazorWasm.Shared
{
	public record LoginFormModel
	{
		public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
		public bool RememberMe { get; set; } = false;        
	}
}
