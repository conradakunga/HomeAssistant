using Assistant.Extensions;
using Assistant.Gpio.Config;
using Assistant.Gpio.Drivers;
using Assistant.Gpio.Events;
using Assistant.Gpio.Events.EventArgs;
using Assistant.Logging;
using Assistant.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using static Assistant.Gpio.Enums;

namespace Assistant.Gpio.Controllers {
	public class PiGpioController {
		private static readonly ILogger Logger = new Logger(typeof(PiGpioController).Name);
		public readonly EGPIO_DRIVERS GpioDriver = EGPIO_DRIVERS.RaspberryIODriver;
		private static bool IsAlreadyInit;
		public static AvailablePins AvailablePins { get; private set; }
		internal static bool IsGracefullShutdownRequested = true;
		public static bool IsAllowedToExecute => IsPiEnvironment();
		private static PinEvents? EventManager;
		private static GpioMorseTranslator? MorseTranslator;
		private static PiBluetoothController? BluetoothController;
		private static PiSoundController? SoundController;
		private static PinController? PinController;
		private static PinConfigManager? ConfigManager;

		public PiGpioController(EGPIO_DRIVERS driverToUse, AvailablePins pins, bool shouldShutdownGracefully) {
			GpioDriver = driverToUse;
			AvailablePins = pins;
			IsGracefullShutdownRequested = shouldShutdownGracefully;
		}

		public async Task InitController() {
			if (IsAlreadyInit) {
				return;
			}

			if (!IsAllowedToExecute) {
				Logger.Warning("Running OS platform is unsupported.");
				return;
			}

			PinController = new PinController();

			switch (GpioDriver) {
				case EGPIO_DRIVERS.RaspberryIODriver:
					PinController.InitPinController<RaspberryIODriver>(new RaspberryIODriver(), NumberingScheme.Logical);
					break;
				case EGPIO_DRIVERS.SystemDevicesDriver:
					PinController.InitPinController<SystemDeviceDriver>(new SystemDeviceDriver(), NumberingScheme.Logical);
					break;
				case EGPIO_DRIVERS.WiringPiDriver:
					PinController.InitPinController<WiringPiDriver>(new WiringPiDriver(), NumberingScheme.Logical);
					break;
			}

			IGpioControllerDriver? driver = PinController.GetDriver();

			if (driver == null || !driver.IsDriverProperlyInitialized) {
				Logger.Warning("Failed to initialize pin controller and its drivers.");
				return;
			}

			EventManager = new PinEvents();
			MorseTranslator = new GpioMorseTranslator();
			BluetoothController = new PiBluetoothController();
			SoundController = new PiSoundController();

			SetEvents();

			if (ConfigManager != null && PinConfigManager.GetConfiguration().PinConfigs.Count <= 0) {
				await ConfigManager.LoadConfiguration().ConfigureAwait(false);
			}

			IsAlreadyInit = true;
		}

		private void SetEvents() {
			if (!IsAllowedToExecute) {
				Logger.Warning("Running OS platform is unsupported.");
				return;
			}

			var driver = PinController.GetDriver();

			if (driver == null || !driver.IsDriverProperlyInitialized) {
				Logger.Warning("Failed to set events as drivers are not loaded.");
				return;
			}

			List<Pin> pinConfigs = new List<Pin>();
			for (int i = 0; i < Constants.BcmGpioPins.Length; i++) {
				Pin? config = PinController.GetDriver()?.GetPinConfig(Constants.BcmGpioPins[i]);

				if (config == null) {
					continue;
				}

				pinConfigs.Add(config);
				Logger.Trace($"Generated pin config for {Pi.Gpio[i].PhysicalPinNumber} gpio pin.");
			}

			ConfigManager = new PinConfigManager().Init(new PinConfig(pinConfigs));

			for (int i = 0; i < AvailablePins.OutputPins.Length; i++) {
				EventManager?.RegisterEvent(new EventConfig(AvailablePins.OutputPins[i], GpioPinMode.Output, GpioPinEventStates.ALL, OutputPinEvents));				
				Logger.Trace($"Event registered for {AvailablePins.OutputPins[i]} gpio pin with Output state.");
			}

			for (int i = 0; i < AvailablePins.InputPins.Length; i++) {
				EventManager?.RegisterEvent(new EventConfig(AvailablePins.InputPins[i], GpioPinMode.Input, GpioPinEventStates.ALL, InputPinEvents));				
				Logger.Trace($"Event registered for {AvailablePins.InputPins[i]} gpio pin with Input state.");
			}
		}

		private void OutputPinEvents(object sender, OnValueChangedEventArgs e) {
			if(sender == null) {
				return;
			}

			Logger.Info($"{e.CurrentMode.ToString()} | {e.Pin} has been set to {e.CurrentState.ToString()} state. ({e.PreviousPinState.ToString()})");
		}

		private void InputPinEvents(object sender, OnValueChangedEventArgs e) {
			if (sender == null) {
				return;
			}

			Logger.Info($"{e.CurrentMode.ToString()} | {e.Pin} has been set to {e.CurrentState.ToString()} state. ({e.PreviousPinState.ToString()})");
		}

		public void Shutdown() {
			if (!IsAlreadyInit) {
				return;
			}

			ConfigManager?.SaveConfig().ConfigureAwait(false);
			EventManager?.StopAllEventGenerators();
			PinController.GetDriver()?.ShutdownDriver();
		}

		private static bool IsPiEnvironment() {
			if (Helpers.GetOsPlatform() == OSPlatform.Linux) {
				return Pi.Info.RaspberryPiVersion.ToString().Equals("Pi3ModelBEmbest", StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}

		public static PinEvents? GetEventManager() => EventManager;
		public static GpioMorseTranslator? GetMorseTranslator() => MorseTranslator;
		public static PiBluetoothController? GetBluetoothController() => BluetoothController;
		public static PiSoundController? GetSoundController() => SoundController;
		public static PinController? GetPinController() => PinController;
	}
}
