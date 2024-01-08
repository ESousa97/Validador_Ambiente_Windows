using System;
using System.Drawing;
using System.Windows.Forms;
using System.Management;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Net;

namespace Padronizador_ambiente_v0._1
{
    public partial class Form1 : Form
    {
        private Label? questionLabel;
        private Button? yesButton;
        private Button? noButton;
        private ListBox? infoListBox; // Adicionado uma ListBox
        public bool UserResponse { get; private set; }

        public Form1()
        {
            InitializeComponent();
            InitializeCustomControls();
            CollectSystemInfo();
        }

        private void InitializeCustomControls()
        {
            // Inicializando e configurando o questionLabel
            questionLabel = new Label
            {
                Text = "A máquina atual está padronizada?",
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold) // Fonte da pergunta
            };
            this.Controls.Add(questionLabel);
            questionLabel.Location = new Point((this.ClientSize.Width - questionLabel.PreferredWidth) / 2, 150); // Centraliza horizontalmente

            // Inicializando e configurando o yesButton
            yesButton = new Button
            {
                Text = "Sim",
                Location = new Point((this.ClientSize.Width / 2) - 105, 250), // Posição ajustada
                Size = new Size(100, 30)
            };
            yesButton.Click += (sender, e) => { UserResponse = true; this.Close(); };
            this.Controls.Add(yesButton);

            // Inicializando e configurando o noButton
            noButton = new Button
            {
                Text = "Não",
                Location = new Point((this.ClientSize.Width / 2) + 5, 250), // Posição ajustada
                Size = new Size(100, 30)
            };
            noButton.Click += (sender, e) => { UserResponse = false; this.Close(); };
            this.Controls.Add(noButton);
        }

        private void CollectSystemInfo()
        {
            try
            {
                // Inicializando e configurando o infoListBox
                infoListBox = new ListBox
                {
                    Location = new Point((this.ClientSize.Width - 200) / 2, 310),
                    Size = new Size(200, 100)
                };
                this.Controls.Add(infoListBox);

                // Adicionando informações básicas do sistema
                infoListBox.Items.Add("Hostname: " + Environment.MachineName);
                infoListBox.Items.Add("Grupos de Trabalho: " + GetWorkgroup());
                infoListBox.Items.Add("Firewall: " + IsFirewallEnabled());
                infoListBox.Items.Add("NetFramework 3.5: " + IsNetFrameworkInstalled("v3.5"));
                infoListBox.Items.Add("NetFramework 4.0: " + IsNetFrameworkInstalled("v4.0"));
                infoListBox.Items.Add("Disco C: Compartilhado: " + IsDriveCShared());

                // Obter e adicionar o endereço IP local
                string ipAddress = GetLocalIPAddress();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    infoListBox.Items.Add("IP: " + ipAddress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao coletar informações do sistema: " + ex.Message);
            }
        }

        private string GetWorkgroup()
        {
            string workgroupName = "Desconhecido";

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    workgroupName = obj["Workgroup"]?.ToString() ?? "Desconhecido";
                    break; // Normalmente só há um resultado
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao obter o grupo de trabalho: " + ex.Message);
            }

            return workgroupName;
        }

        private string IsFirewallEnabled()
        {
            try
            {
                // Crie um objeto de gerenciamento para acessar as configurações do Firewall do Windows
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service WHERE Name='MpsSvc'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["State"] != null && obj["State"].ToString() == "Running")
                    {
                        return "Habilitado";
                    }
                }
                return "Desabilitado";
            }
            catch (Exception ex)
            {
                return "Erro: " + ex.Message;
            }
        }

#pragma warning disable CS8600 // Conversão de literal nula ou possível valor nulo em tipo não anulável.

        private string IsNetFrameworkInstalled(string minimumVersion)
        {
            try
            {
                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP"))
                {
                    if (ndpKey != null)
                    {
                        string[] subKeys = ndpKey.GetSubKeyNames();
                        foreach (string subKey in subKeys)
                        {
                            if (subKey.StartsWith("v") && subKey.CompareTo(minimumVersion) >= 0)
                            {
                                RegistryKey versionKey = ndpKey.OpenSubKey(subKey);
                                if (versionKey != null)
                                {
                                    object installValueObj = versionKey.GetValue("Install");
                                    int? installValue = installValueObj as int?;
                                    if (installValue.HasValue && installValue == 1)
                                    {
                                        return "Instalado";
                                    }
                                }
                            }
                        }
                    }
                }
                return "Não Instalado";
            }
            catch (Exception ex)
            {
                return "Erro: " + ex.Message;
            }
        }

#pragma warning restore CS8600 // Restaurar avisos relacionados a CS8600




        private string IsDriveCShared()
        {
            try
            {
                // Crie um objeto de gerenciamento para acessar informações sobre compartilhamento do disco C:
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Share WHERE Name='C$'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return "Compartilhado";
                }
                return "Não Compartilhado";
            }
            catch (Exception ex)
            {
                return "Erro: " + ex.Message;
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                foreach (IPAddress address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }

                return "Nenhum endereço IPv4 encontrado";
            }
            catch (Exception ex)
            {
                return "Erro: " + ex.Message;
            }
        }
    }
}
