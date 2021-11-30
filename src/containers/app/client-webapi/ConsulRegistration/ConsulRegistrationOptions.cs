using System.Collections.Generic;

namespace Ais.Common.ServiceRegistration
{
	public class ConsulRegistrationOptions
	{
		public string ConsulAddress { get; set; }

		public string ServiceName { get; set; }

		public string RegistrationToken { get; set; }

		public int DeregisterRegisterDelayInSec { get; set; } = 100;

		public bool RegisterInConsul { get; set; } = false;
	}
}
