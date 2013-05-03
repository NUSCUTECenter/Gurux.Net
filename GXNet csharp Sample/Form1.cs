//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/Old/GuruxNet/GXNet%20csharp%20Sample/Form1.cs $
//
// Version:         $Revision: 3587 $,
//                  $Date: 2011-05-02 12:02:16 +0300 (ma, 02 touko 2011) $
//                  $Author: kurumi $
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
// 
// More information of Gurux Network solution (TCP/IP and UDP) : http://www.gurux.org/GXNet
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using Gurux.Net;
using System.Diagnostics;
using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Data;
using System.Text;
using Gurux.Common;

namespace GXNetSample
{
    internal partial class Form1 : System.Windows.Forms.Form
    {
        GXNet gxNet1;
        int cnt = 0;
        #region Close
        /// <summary>
        /// Closes network connection.
        /// </summary>
        /// <param name="eventSender"></param>
        /// <param name="eventArgs"></param>
        private void CloseBtn_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                gxNet1.Close();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        #endregion //Close

        /// <summary>
        /// Set initial settings.
        /// </summary>
        /// <param name="eventSender"></param>
        /// <param name="eventArgs"></param>
        private void Form1_Load(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                gxNet1 = new GXNet();
                gxNet1.Settings = GXNetSample.Properties.Settings.Default.MediaSetting;
                gxNet1.Trace = TraceLevel.Verbose;
                gxNet1.OnTrace += new TraceEventHandler(gxNet1_OnTrace);
                gxNet1.OnError += new Gurux.Common.ErrorEventHandler(gxNet1_OnError);
                gxNet1.OnReceived += new ReceivedEventHandler(gxNet1_OnReceived);
                gxNet1.OnMediaStateChange += new MediaStateChangeEventHandler(gxNet1_OnMediaStateChange);
                gxNet1.OnClientConnected += new ClientConnectedEventHandler(gxNet1_OnClientConnected);
                gxNet1.OnClientDisconnected += new ClientDisconnectedEventHandler(gxNet1_OnClientDisconnected);
                gxNet1.Protocol = NetworkType.Tcp;
                if (gxNet1.IsOpen)
                {
                    gxNet1_OnMediaStateChange(gxNet1, new MediaStateEventArgs(MediaState.Open));
                }
                else
                {
                    gxNet1_OnMediaStateChange(gxNet1, new MediaStateEventArgs(MediaState.Closed));
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        void gxNet1_OnTrace(object sender, TraceEventArgs e)
        {
            if ((e.Type & TraceTypes.Info) != 0)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            else if ((e.Type & TraceTypes.Error) != 0)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            else if ((e.Type & TraceTypes.Warning) != 0)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            else if ((e.Type & TraceTypes.Sent) != 0)
            {
                System.Diagnostics.Debug.WriteLine("<- " + e.ToString());
            }
            else if ((e.Type & TraceTypes.Received) != 0)
            {
                System.Diagnostics.Debug.WriteLine("-> " + e.ToString());
            }            
        }       

        /// <summary>
        /// When new client has connected in server mode.
        /// </summary>
        private void gxNet1_OnClientConnected(object sender, ConnectionEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new ClientConnectedEventHandler(gxNet1_OnClientConnected), new object[] { sender, e });
                }
                else
                {
                    ClientsList.Items.Add(e.Info);
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        /// <summary>
        /// When new client has disconnected in server mode.
        /// </summary>
        private void gxNet1_OnClientDisconnected(object sender, ConnectionEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new ClientDisconnectedEventHandler(gxNet1_OnClientDisconnected), new object[] { sender, e });
                }
                else
                {
                    //Find client and remove it from the list.
                    int pos = ClientsList.FindStringExact(e.Info);
                    if (pos != -1)
                    {
                        ClientsList.Items.RemoveAt(pos);
                    }
                    else
                    {
                        ErrorList.Items.Add("Failed to find item: " + e.Info);
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        #region OnError

        /// <summary>
        /// Show occured error.
        /// </summary>
        private void gxNet1_OnError(object sender, Exception ex)
        {
            try
            {
                gxNet1.Close();
                MessageBox.Show(ex.Message);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        #endregion //OnError

        #region OnMediaStateChange
        private void gxNet1_OnMediaStateChange(object sender, MediaStateEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new MediaStateChangeEventHandler(gxNet1_OnMediaStateChange), new object[] { sender, e });
                }
                else
                {
                    bool bOpen;
                    bOpen = e.State == Gurux.Common.MediaState.Open;
                    HexCB.Enabled = !bOpen;
                    OpenBtn.Enabled = !bOpen;
                    SendText.Enabled = bOpen;
                    SendBtn.Enabled = bOpen;
                    CloseBtn.Enabled = bOpen;
                    ReceivedText.Enabled = bOpen;
                    //Close interval timer if media is closed.
                    if (!bOpen)
                    {
                        IntervalTB.Enabled = false;
                    }
                    else
                    {
                        IntervalTB.Enabled = true;
                    }
                    //Enable echo if media is open and server.
                    EchoCB.Enabled = gxNet1.Server && bOpen;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        #endregion //OnMediaStateChange

        #region OnReceived
        /// <summary>
        /// Show received data.
        /// </summary>
        private void gxNet1_OnReceived(object sender, ReceiveEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new ReceivedEventHandler(gxNet1_OnReceived), new object[] { sender, e });
                }
                else
                {
                    //Echo received text.
                    if (EchoCB.Checked)
                    {
                        gxNet1.Send(e.Data, e.SenderInfo);
                        ReceivedText.Text = "";
                    }
                    //We receive byte array from GXNet and this must be changed to chars.
                    if (HexCB.Checked)
                    {
                        ++cnt;
                        ReceivedText.Text += BitConverter.ToString((byte[])e.Data);
                    }
                    else
                    {
                        // Gets received data as string.
                        ReceivedText.Text += System.Text.Encoding.ASCII.GetString((byte[])e.Data);
                    }
                }
            }
            catch (Exception Ex)
            {
                ErrorList.Items.Add(Ex.Message);
            }
        }
        #endregion //OnReceived

        /// <summary>
        /// Start read items with selected interval (ms).
        /// </summary>
        /// <param name="eventSender"></param>
        /// <param name="eventArgs"></param>
        private void IntervalBtn_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                IntervalTimer.Interval = int.Parse(IntervalTB.Text);
                IntervalTB.Enabled = IntervalTimer.Enabled;
                IntervalTimer.Enabled = !IntervalTimer.Enabled;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        /// <summary>
        /// Read selected item.
        /// </summary>
        /// <param name="eventSender"></param>
        /// <param name="eventArgs"></param>
        private void IntervalTimer_Tick(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                if (!gxNet1.IsOpen)
                {
                    gxNet1.Open();
                    System.Threading.Thread.Sleep(400);
                    gxNet1.Close();
                    return;
                }
                SendBtn_Click(SendBtn, new System.EventArgs());
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        #region Open
        /// <summary>
        /// Open Network connection.
        /// </summary>
        private void OpenBtn_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                gxNet1.Open();                
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        #endregion //Open

        /// <summary>
        /// Update packet counters.
        /// </summary>
        /// <param name="eventSender"></param>
        /// <param name="eventArgs"></param>
        private void PacketCounterTimer_Tick(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                ReceivedTB.Text = gxNet1.BytesReceived.ToString();
                SentTB.Text = gxNet1.BytesSent.ToString();
                gxNet1.ResetByteCounters();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        #region Properties
        /// <summary>
        /// Show GXNet media properties.
        /// </summary>
        private void PropertiesBtn_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                if (gxNet1.Properties(this))
                {
                    GXNetSample.Properties.Settings.Default.MediaSetting = gxNet1.Settings;
                    GXNetSample.Properties.Settings.Default.Save();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        #endregion //Properties

        #region Send
        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="eventSender"></param>
        /// <param name="eventArgs"></param>
        private void SendBtn_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                ReceivedText.Text = string.Empty;
                if (SyncBtn.Checked) // Sends data synchronously.
                {
                    if (HexCB.Checked)
                    {
                        // Sends data as byte array.                        
                        lock (gxNet1.Synchronous)
                        {
                            Gurux.Common.ReceiveParameters<byte[]> p = new Gurux.Common.ReceiveParameters<byte[]>()
                            {
                                WaitTime = Convert.ToInt32(WaitTimeTB.Text),
                                Eop = EOPText.Text,                                
                            };
                            gxNet1.Send(ASCIIEncoding.ASCII.GetBytes(SendText.Text), null);
                            if (gxNet1.Receive(p))
                            {
                                ReceivedText.Text = Convert.ToString(p.Reply);
                            }
                        }
                    }
                    else
                    {
                        // Sends data as ASCII string.
                        lock (gxNet1.Synchronous)
                        {
                            Gurux.Common.ReceiveParameters<string> p = new Gurux.Common.ReceiveParameters<string>()
                            {
                                WaitTime = Convert.ToInt32(WaitTimeTB.Text),
                                Eop = EOPText.Text,                                
                            };
                            gxNet1.Send(SendText.Text, null);
                            if (gxNet1.Receive(p))
                            {
                                ReceivedText.Text = Convert.ToString(p.Reply);
                            }
                        }
                    }
                }
                else // Sends data asynchronously.
                {
                    if (HexCB.Checked)
                    {
                        // Sends data as byte array.
                        gxNet1.Send(ASCIIEncoding.ASCII.GetBytes(SendText.Text), null);
                    }
                    else
                    {
                        // Sends data as ASCII string.
                        gxNet1.Send(SendText.Text, null);
                    }
                }
            }
            catch (Exception Ex)
            {
                //disable timer is exists
                if (IntervalTimer.Enabled)
                {
                    IntervalBtn_Click(IntervalBtn, new System.EventArgs());
                }
                MessageBox.Show(Ex.Message);
            }
        }
        #endregion //Send
    }
}