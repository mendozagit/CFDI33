using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace CFD33
{
    public partial class Form1 : Form
    {


        //Obtener numero de certificado
        string RutaFE;
        string RutaCer;
        string RutaKey;
        string RutaCO;
        string FacturaActual;
        string RutaComprobantes;
        string ClavePrivada;
        public Form1()
        {
            InitializeComponent();
            RutaFE = @"C:\Dympos\FacturaElectronica\";
            RutaCer = RutaFE + @"Certificados\CSD01_AAA010101AAA.cer";
            RutaKey = RutaFE + @"Certificados\CSD01_AAA010101AAA.key";
            RutaCO = RutaFE + @"Certificados\cadenaoriginal_3_3.xslt";
            RutaComprobantes = RutaFE + @"Comprobantes\";
            ClavePrivada = "12345678a";
            FacturaActual = "";
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            //Obtener las variables
            string NoCertificado, aa, b, c;
            SelloDigital.leerCER(RutaCer, out aa, out b, out c, out NoCertificado);

            //Llenamos la clase COMPROBANTE--------------------------------------------------------
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "3.3";
            oComprobante.Serie = "H";
            oComprobante.Folio = "1";
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            // oComprobante.Sello = "faltante"; //sig video
            oComprobante.FormaPago = "99";
            oComprobante.NoCertificado = NoCertificado;
            // oComprobante.Certificado = ""; //sig video
            oComprobante.SubTotal = 10m;
            oComprobante.Descuento = 1;
            oComprobante.Moneda = "MXN";
            oComprobante.Total = 9;
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = "PUE";
            oComprobante.LugarExpedicion = "20131";

            ComprobanteEmisor oEmisor = new ComprobanteEmisor();

           // oEmisor.Rfc = "MEJJ940824C61";
            oEmisor.Rfc = "AAA010101AAA"; //los sellos estan utilizando este rfc
            oEmisor.Nombre = "JESUS MENDOZA JUAREZ";
            oEmisor.RegimenFiscal = "601";

            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = "DAVID RODRIGUEZ BALCAZAR";
            oReceptor.Rfc = "ROBD8901188E1";
            oReceptor.UsoCFDI = "P01";

            //asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;


            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            oConcepto.Importe = 10m;
            oConcepto.ClaveProdServ = "10101505";
            oConcepto.Cantidad = 1;
            oConcepto.ClaveUnidad = "C81";
            oConcepto.Descripcion = "Un misil para la guerra";
            oConcepto.ValorUnitario = 10m;
            oConcepto.Descuento = 1;


            lstConceptos.Add(oConcepto);
            oComprobante.Conceptos = lstConceptos.ToArray();
            //lstConceptos.Add(oConcepto); estas agregando 2 veces el concepto y tu total no corresponde 
            //oComprobante.Conceptos = lstConceptos.ToArray();

            // FacturaActual = oComprobante.Serie + oComprobante.Folio + "_" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "_" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + ".xml";
            FacturaActual = "FACTURA_" + oComprobante.Serie + oComprobante.Folio + ".xml";

            //Crear Xml
            CreateXML(oComprobante);



            string cadenaOriginal = "";

            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(RutaCO);

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
            {

                transformador.Transform(RutaComprobantes + FacturaActual, xwo);
                cadenaOriginal = sw.ToString();
            }


            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(RutaCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, RutaKey, ClavePrivada);

            CreateXML(oComprobante);

            //TIMBRE DEL XML
            WSTimbrado.RespuestaCFDi respuestaCFDI = new WSTimbrado.RespuestaCFDi();

            byte[] bXML = System.IO.File.ReadAllBytes(RutaComprobantes + FacturaActual);

            WSTimbrado.TimbradoClient oTimbrado = new WSTimbrado.TimbradoClient();

            respuestaCFDI = oTimbrado.TimbrarTest("DGE131017IP1", "9616fb2b81e89673495f", bXML);

            if (respuestaCFDI.Documento == null)
            {
                MessageBox.Show(respuestaCFDI.Mensaje);
            }
            else
            {

                File.WriteAllBytes(RutaComprobantes + FacturaActual, respuestaCFDI.Documento);
                MessageBox.Show("Ok");

            }




        }


        private void CreateXML(Comprobante oComprobante)
        {
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            xmlNameSpace.Add("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));
            string sXml = "";
            using (var sww = new StringWriterEncoding(Encoding.UTF8))
            {
                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    oXmlSerializar.Serialize(writter, oComprobante, xmlNameSpace);
                    sXml = sww.ToString();
                }
            }
            //guardamos el string en un archivo
            System.IO.File.WriteAllText(RutaComprobantes + FacturaActual, sXml);
        }




    }
}
