using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace PuntoDeVenta.src
{
    public class Temas
    {
        public ResourceDictionary CargarTema()
        {
            string tema = Properties.Settings.Default.Tema;
            
            if(string.IsNullOrEmpty(tema))
            {
                tema = "Green";
                Properties.Settings.Default.Tema = tema;
                Properties.Settings.Default.Save();
            }

            // Usar Pack URI absoluto para asegurar que se encuentre el recurso embebido
            string packUri = $"pack://application:,,,/PuntoDeVenta;component/src/Themes/{tema}.xaml";

            ResourceDictionary resDic = new ResourceDictionary
            {
                Source = new Uri(packUri, UriKind.Absolute)
            };

            return resDic;
        }
    }
}
