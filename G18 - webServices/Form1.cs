using System;
using System.Collections.Generic;
using System.Net.Http; //Se agregó la libreria Http
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json; //Se agregró la libreria Json

namespace G18___webServices
{
    public partial class Form1 : Form
    {
        public class WeatherResponse
        {
            public List<CurrentCondition>? current_condition { get; set; }
            public List<NearestArea>? nearest_area { get; set; }
        }

        public class CurrentCondition
        {
            public string? Temp_C { get; set; }
            public string? FeelsLikeC { get; set; }
            public string? Humidity { get; set; }
            public string? WindspeedKmph { get; set; }
            public List<WeatherDesc>? WeatherDesc { get; set; }
        }

        public class WeatherDesc
        {
            public string? value { get; set; }
        }

        public class NearestArea
        {
            public List<AreaName>? areaName { get; set; }
            public List<Country>? country { get; set; }
        }

        public class AreaName
        {
            public string? value { get; set; }
        }

        public class Country
        {
            public string? value { get; set; }
        }

        private readonly HttpClient httpClient = new HttpClient();
        private const string SunIconPath = "sun_icon.png";
        private const string CloudIconPath = "cloud_icon.png";
        private const string RainIconPath = "rain_icon.png";
        private const string DefaultIconPath = "default_icon.png";

        private Image? sunIcon;
        private Image? cloudIcon;
        private Image? rainIcon;
        private Image? defaultIcon;

        public Form1()
        {
            InitializeComponent();
            CargarIconos();

            Color colorTransparente = Color.FromArgb(128, 106, 90, 205);
            panel1.BackColor = colorTransparente;
            panel2.BackColor = colorTransparente;
            label2.BackColor = colorTransparente;
            label3.BackColor = colorTransparente;
            label4.BackColor = colorTransparente;
            label5.BackColor = colorTransparente;
            label6.BackColor = colorTransparente;
            label7.BackColor = colorTransparente;
            pictureBox1.BackColor = colorTransparente;
            pictureBox1.Visible = false;
        }

        private void CargarIconos()
        {
            try
            {
                sunIcon = File.Exists(SunIconPath) ? Image.FromFile(SunIconPath) : null;
                cloudIcon = File.Exists(CloudIconPath) ? Image.FromFile(CloudIconPath) : null;
                rainIcon = File.Exists(RainIconPath) ? Image.FromFile(RainIconPath) : null;
                defaultIcon = File.Exists(DefaultIconPath) ? Image.FromFile(DefaultIconPath) : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar iconos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarDatos()
        {
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";
            label7.Text = "";
            pictureBox1.Image = null;
        }

        private async Task<WeatherResponse> ObtenerDatosClimaticos(string cityName)
        {
            string weatherUrl = $"https://wttr.in/{cityName}?format=j1";
            HttpResponseMessage response = await httpClient.GetAsync(weatherUrl);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Error en la respuesta de la API.");

            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(jsonResponse) ?? new WeatherResponse();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string cityName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(cityName) || !Regex.IsMatch(cityName, @"^[a-zA-Z\s,á-úÁ-Ú\s]+$"))
            {
                MessageBox.Show("Por favor, ingrese una ciudad válida", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LimpiarDatos();
            button1.Enabled = false;
            button1.Text = "Cargando...";
            button2.Enabled = false;

            try
            {
                var weatherData = await ObtenerDatosClimaticos(cityName);

                if (weatherData.current_condition != null && weatherData.current_condition.Count > 0)
                {
                    var currentTemp = weatherData.current_condition[0].Temp_C ?? "N/A";
                    var feelsLike = weatherData.current_condition[0].FeelsLikeC ?? "N/A";
                    var humidity = weatherData.current_condition[0].Humidity ?? "N/A";
                    var windSpeed = weatherData.current_condition[0].WindspeedKmph ?? "N/A";
                    var weatherDescription = weatherData.current_condition[0].WeatherDesc?.FirstOrDefault()?.value?.ToLower() ?? "desconocido";

                    string actualCityName = weatherData.nearest_area?[0]?.areaName?[0]?.value ?? "Ciudad desconocida";
                    string countryName = weatherData.nearest_area?[0]?.country?[0]?.value ?? "País desconocido";
                    panel2.Visible = true;

                    label2.Text = $"{actualCityName}, {countryName}";
                    label3.Text = $"{currentTemp}°C";
                    label4.Text = $"Sensación térmica: {feelsLike}°C";
                    label5.Text = $"Humedad: {humidity}%";
                    label6.Text = $"Viento: {windSpeed} km/h";
                    pictureBox1.Visible = true;

                    if (weatherDescription.Contains("sunny"))
                    {
                        pictureBox1.Image = sunIcon != null ? new Bitmap(sunIcon, new Size(150, 150)) : null;
                        label7.Text = "Descripción: soleado";
                    }
                    else if (weatherDescription.Contains("cloudy"))
                    {
                        pictureBox1.Image = cloudIcon != null ? new Bitmap(cloudIcon, new Size(150, 150)) : null;
                        label7.Text = "Descripción: nublado";
                    }
                    else if (weatherDescription.Contains("rain"))
                    {
                        pictureBox1.Image = rainIcon != null ? new Bitmap(rainIcon, new Size(150, 150)) : null;
                        label7.Text = "Descripción: lluvioso";
                    }
                    else
                    {
                        pictureBox1.Image = defaultIcon != null ? new Bitmap(defaultIcon, new Size(150, 150)) : null;
                        label7.Text = "Descripción: -";
                    }
                }
                else
                {
                    MessageBox.Show($"No se pudo encontrar información sobre el clima para la ciudad: {cityName}.", "Ciudad no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Se produjo un error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button1.Enabled = true;
                button1.Text = "Obtener Clima";
                button2.Enabled = true;
                button2.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(button1, "Haz clic para obtener el clima de la ciudad ingresada.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LimpiarDatos();
        }
    }
}
