using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kepware.ClientAce.OpcDaClient;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Kepware.ClientAce.OpcDaClient.DaServerMgt DAserver = new Kepware.ClientAce.OpcDaClient.DaServerMgt();
        Kepware.ClientAce.OpcDaClient.ConnectInfo connectInfo = new Kepware.ClientAce.OpcDaClient.ConnectInfo();
        bool connectFailed;
        int activeServerSubscriptionHandle;
        int clientSubscriptionHandle;
        ItemIdentifier[] itemIdentifiers = new ItemIdentifier[5];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "opcda://localhost/Kepware.KEPServerEX.V6/{B3AF0BF6-4C0C-4804-A122-6F3B160F4397}";

            connectInfo.LocalId = "en";
            connectInfo.KeepAliveTime = 1000;
            connectInfo.RetryAfterConnectionError = true;
            connectInfo.RetryInitialConnection = false;
            connectInfo.ClientName = "CS Simple Client";
            connectFailed = false;

            int clientHandle = 1;

            try
            {
                DAserver.Connect(url, clientHandle, ref connectInfo, out connectFailed);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Handled Connect exception. Reason: " + ex.Message);

                connectFailed = true;
            }

            if (connectFailed)
            {
                MessageBox.Show("Connect failed");
            }

            SubscribeToOPCDAServerEvents();

            Subscribe2Data();
            ModifySubscription(true);
        }

        private void ModifySubscription(bool action)
        {
            DAserver.SubscriptionModify(activeServerSubscriptionHandle, action);
        }

        private void Subscribe2Data()
        {
            int itemIndex;

            clientSubscriptionHandle = 1;

            bool active = false;

            int updateRate = 1000;

            Single deadBand = 0;

            int revisedUpdateRate;

            itemIdentifiers[0] = new ItemIdentifier();
            itemIdentifiers[0].ClientHandle = 0;
            itemIdentifiers[0].DataType = Type.GetType("System.Boolean");
            itemIdentifiers[0].ItemName = "Channel1.Device1.Bool1";
           
            itemIdentifiers[1] = new ItemIdentifier();
            itemIdentifiers[1].ClientHandle = 1;
            itemIdentifiers[1].DataType = Type.GetType("System.Boolean");
            itemIdentifiers[1].ItemName = "Channel1.Device1.Bool2";

            try
            {
                DAserver.Subscribe(clientSubscriptionHandle, active, updateRate, out revisedUpdateRate, deadBand, ref itemIdentifiers, out activeServerSubscriptionHandle);

                for (itemIndex = 0; itemIndex <= 1; itemIndex++)
                {
                    if (itemIdentifiers[itemIndex].ResultID.Succeeded == false)
                    {
                        MessageBox.Show("Faiked to add item" + itemIdentifiers[itemIndex].ItemName + "to subscription");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Handled Subscribe exception. Reason: " + ex.Message);
            }
        }

        private void SubscribeToOPCDAServerEvents()
        {
            DAserver.DataChanged += new DaServerMgt.DataChangedEventHandler(DAserver_DataChanged);
            DAserver.ServerStateChanged += new DaServerMgt.ServerStateChangedEventHandler(DAserver_ServerStateChanged);
        }

        private void DAserver_ServerStateChanged(int clientHandle, ServerState state)
        {
            object[] SSCevHndlrArray = new object[2];
            SSCevHndlrArray[0] = clientHandle;
            SSCevHndlrArray[1] = state;
            BeginInvoke(new DaServerMgt.ServerStateChangedEventHandler(ServerStateChanged), SSCevHndlrArray);
        }

        private void ServerStateChanged(int clientHandle, ServerState state)
        {
            try
            {
                switch (state)
                {
                    case ServerState.ERRORSHUTDOWN:
                        MessageBox.Show("The server is shutting down. The client has automatically disconnected.");
                        break;
                    case ServerState.ERRORWATCHDOG:
                        MessageBox.Show("Server has been lost. Client will keep attempting to reconnect. ");
                        break;
                    case ServerState.CONNECTED:
                        MessageBox.Show("ServerStateChanged, connected");
                        break;
                    case ServerState.DISCONNECTED:
                        MessageBox.Show("ServerStateChanged, disconnected");
                        break;
                    default:
                        MessageBox.Show("ServerStateChanged, undefined state found. ");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Handled Serve State changed exception. Reason: " + ex.Message);
            }
        }

        private void DAserver_DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            object[] DCevHndlrArray = new object[4];
            DCevHndlrArray[0] = clientSubscription;
            DCevHndlrArray[1] = allQualitiesGood;
            DCevHndlrArray[2] = noErrors;
            DCevHndlrArray[3] = itemValues;
            BeginInvoke(new DaServerMgt.DataChangedEventHandler(DataChanged), DCevHndlrArray);
        }

        private void DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            try
            {
                foreach (ItemValueCallback itemValue in itemValues)
                {
                    int itemIndex = (int)itemValue.ClientHandle;
                    switch (itemIndex)
                    {
                        case 0:
                            if (itemValue.Value == null)
                            {
                                checkBox1.Checked = false;
                            }
                            else
                            {
                                checkBox1.Checked = Convert.ToBoolean(itemValue.Value);
                            }
                            break;
                        case 1:
                            if (itemValue.Value == null)
                            {
                                checkBox2.Checked = false;
                            }
                            else
                            {
                                checkBox2.Checked = Convert.ToBoolean(itemValue.Value);
                            }
                            break;
                        case 2:
                            if (itemValue.Value == null)
                            {
                                textBox1.Text = "Unknown";
                            }
                            else
                            {
                                textBox1.Text = itemValue.Value.ToString();
                            }
                            break;
                        case 3:
                            if (itemValue.Value == null)
                            {
                                textBox2.Text = "Unknown";
                            }
                            else
                            {
                                textBox2.Text = itemValue.Value.ToString();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Handled Data Changed exception. Reason: " + ex.Message);
            }
        }
    }
}
