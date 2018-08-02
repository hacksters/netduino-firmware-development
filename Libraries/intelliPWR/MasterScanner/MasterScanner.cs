/*
 * I2C SCANNER - 20.07.2018
 * 
 * =============================================================================
 *
 * The MIT License (MIT)
 * 
 * Copyright (c) 2018 Berk Altun - vberkaltun.com
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sub license, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * =============================================================================
 */

using System;
using Microsoft.SPOT.Hardware;

namespace intelliPWR.MasterScanner
{
    public class MasterScanner : Constant, IMasterScanner
    {
        #region Variable

        protected I2CDevice.Configuration Configuration;
        protected I2CDevice Device;

        protected SSlave Slave;
        protected SConfig Config;

        public ConnectedEventHandler OnConnected;
        public DisconnectedEventHandler OnDisconnected;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MasterScanner()
        {
            // Initialize for first run
            Initialize(DEFAULT_START_ADDRESS, DEFAULT_STOP_ADDRESS, DEFAULT_DEVICE_CLOCK, DEFAULT_DEVICE_TIMEOUT, DEFAULT_DEVICE_RETRY);
        }

        /// <summary>
        /// Default constructor with start and stop parameters.
        /// </summary>
        /// <param name="startAddress">The start address of I2C scanner bus.</param>
        /// <param name="stopAddress">The stop address of I2C scanner bus.</param>
        public MasterScanner(byte startAddress, byte stopAddress)
        {
            // Initialize for first run
            Initialize(startAddress, stopAddress, DEFAULT_DEVICE_CLOCK, DEFAULT_DEVICE_TIMEOUT, DEFAULT_DEVICE_RETRY);
        }

        /// <summary>
        /// Default constructor with clockSpeed and timeout parameters.
        /// </summary>
        /// <param name="clockSpeed">The clock speed of master scanner lib.</param>
        /// <param name="timeout">Time delay after an one clock hertz.</param>
        public MasterScanner(ushort clockSpeed, ushort timeout)
        {
            // Initialize for first run
            Initialize(DEFAULT_START_ADDRESS, DEFAULT_STOP_ADDRESS, clockSpeed, timeout, DEFAULT_DEVICE_RETRY);
        }

        /// <summary>
        /// Default constructor with clockSpeed, timeout and retryCount parameters.
        /// </summary>
        /// <param name="clockSpeed">The clock speed of master scanner lib.</param>
        /// <param name="timeout">Time delay after an one clock hertz.</param>
        /// <param name="retryCount">Retry count for worst case operations.</param>
        public MasterScanner(ushort clockSpeed, ushort timeout, ushort retryCount)
        {
            // Initialize for first run
            Initialize(DEFAULT_START_ADDRESS, DEFAULT_STOP_ADDRESS, clockSpeed, timeout, retryCount);
        }
        /// <summary>
        /// Default constructor with startAddress, stopAddress, clockSpeed, timeout and retryCount parameters.
        /// </summary>
        /// <param name="startAddress">The start address of I2C scanner bus.</param>
        /// <param name="stopAddress">The stop address of I2C scanner bus.</param>
        /// <param name="clockSpeed">The clock speed of master scanner lib.</param>
        /// <param name="timeout">Time delay after an one clock hertz.</param>
        /// <param name="retryCount">Retry count for worst case operations.</param>
        public MasterScanner(byte startAddress, byte stopAddress, ushort clockSpeed, ushort timeout, ushort retryCount)
        {
            // Initialize for first run
            Initialize(startAddress, stopAddress, clockSpeed, timeout, retryCount);
        }

        #endregion

        #region Protected

        /// <summary>
        /// Default constructor initializer.
        /// </summary>
        /// <param name="startAddress">The start address of I2C scanner bus.</param>
        /// <param name="stopAddress">The stop address of I2C scanner bus.</param>
        /// <param name="clockSpeed">The clock speed of master scanner lib.</param>
        /// <param name="timeout">Time delay after an one clock hertz.</param>
        /// <param name="retryCount">Retry count for worst case operations.</param>
        protected void Initialize(byte startAddress, byte stopAddress, ushort clockSpeed, ushort timeout, ushort retryCount)
        {
            Configuration = new I2CDevice.Configuration(0, clockSpeed);
            Device = new I2CDevice(Configuration);

            Slave = new SSlave(startAddress, stopAddress);
            Config = new SConfig(clockSpeed, timeout, retryCount);
        }

        /// <summary>
        /// Check if user did not register an event delegaterary.
        /// </summary>
        /// <param name="array">An array of connected device list.</param>
        /// <param name="count">Size of connected device.</param>
        protected void OnTriggeredConnected(string data)
        {
            if (data != null)
            {
                // We are passing here our data as a string type data but that 
                // Is not acceptable for end-user. So we will trim it with space
                // And after that will store this data in an string array
                string[] connectedOutput = data.Trim().Split(' ');

                // don't bother if user hasn't registered a callback
                if (OnConnected == null)
                    return;

                OnConnected(connectedOutput, (byte)connectedOutput.Length);

                //Debug.Print("=== OnConnected");

                //for (int i = 0; i < connectedOutput.Length; i++)
                //    Debug.Print(connectedOutput[i]);
            }
        }

        /// <summary>
        /// Check if user did not register an event delegaterary.
        /// </summary>
        /// <param name="array">An array of disconnected device list.</param>
        /// <param name="count">Size of disconnected device.</param>
        protected void OnTriggeredDisconnected(string data)
        {
            if (data != null)
            {
                // We are passing here our data as a string type data but that 
                // Is not acceptable for end-user. So we will trim it with space
                // And after that will store this data in an string array
                string[] disconnectedOutput = data.Trim().Split(' ');

                // don't bother if user hasn't registered a callback
                if (OnDisconnected == null)
                    return;

                OnDisconnected(disconnectedOutput, (byte)disconnectedOutput.Length);

                //Debug.Print("=== OnDisconnected");

                //for (int i = 0; i < disconnectedOutput.Length; i++)
                //    Debug.Print(disconnectedOutput[i]);
            }
        }

        #endregion

        #region Public

        public void ScanSlaves()
        {
            // That is looking worst but it is very easy solution for up-to-date
            // Scanning process. In scanning step, we will detect last changes
            // On I2C bus and will put last changed to here as connected or not.
            // After that, we will trim and split it with space delimiter.
            // So, we can generate up-to-date output I2C data's
            string currentConnectedSlavesArray = null;
            string currentDisconnectedSlavesArray = null;

            // Start to scanning slave device on I2C bus
            for (byte address = Slave.StartAddress; address < Slave.StopAddress; address++)
            {
                // Reinitialize config data of an I2C device
                Device.Config = new I2CDevice.Configuration(address, Config.ClockSpeed);
                byte[] handshake = new byte[] { address };

                try
                {
                    // Generate a transaction for testing an connected device
                    I2CDevice.I2CTransaction[] transaction = { I2CDevice.CreateWriteTransaction(handshake) };
                    ushort retryCount = 0;

                    // When retry is overflowed, abort it to worst case
                    while (Device.Execute(transaction, Config.Timeout) != handshake.Length)
                        if (retryCount++ >= Config.RetryCount)
                            throw new Exception();

                    // Check that is it connected as before or not
                    if (Slave.ConnectedSlavesArray[address] == false)
                    {
                        currentConnectedSlavesArray += " " + address.ToString();
                        Slave.ConnectedSlavesArray[address] = true;
                    }
                }
                catch (Exception)
                {
                    // Check that is it disconnected as before or not
                    if (Slave.ConnectedSlavesArray[address] == true)
                    {
                        currentDisconnectedSlavesArray += " " + address.ToString();
                        Slave.ConnectedSlavesArray[address] = false;
                    }
                }
            }

            // Notify end user with delegate method
            OnTriggeredConnected(currentConnectedSlavesArray);
            OnTriggeredDisconnected(currentDisconnectedSlavesArray);

            // Issue 53 - Recommendation by NevynUK. At the here, we are 
            // Disposing our device for next process and reinitializing it. When
            // We choose to not to do that all process about scanning will stop 
            // In next step. We experienced this situation at before
            Device.Dispose();
            Configuration = new I2CDevice.Configuration(0, Config.ClockSpeed);
            Device = new I2CDevice(Configuration);

            //Debug.EnableGCMessages(true);
            //Debug.Print(Debug.GC(false).ToString());        
        }

        public bool SetRange(byte startAddress, byte stopAddress)
        {
            return Slave.SetRange(startAddress,stopAddress);
        }

        public void ResetRange()
        {
            Slave.ResetRange();
        }

        public bool IsConnected(byte address)
        {
            return Slave.IsConnected(address);
        }

        #endregion
    }
}