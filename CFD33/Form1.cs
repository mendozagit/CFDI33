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
            Comprobante comprobante = new Comprobante();
            comprobante.Version = "3.3";
            comprobante.Serie = "H";
            comprobante.Folio = "1";
            comprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            comprobante.FormaPago = "99";
            comprobante.NoCertificado = NoCertificado;

            comprobante.SubTotal = 120;
            comprobante.Descuento = 20;
            comprobante.Moneda = "MXN";
            comprobante.Total = 119;


            comprobante.TipoDeComprobante = "I";
            comprobante.MetodoPago = "PUE";
            comprobante.LugarExpedicion = "20131";
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
            comprobante.Emisor = oEmisor;
            comprobante.Receptor = oReceptor;

            List<ComprobanteConcepto> conceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto cocacola = new ComprobanteConcepto();

            cocacola.ClaveProdServ = "10101505";
            cocacola.ClaveUnidad = "C81";
            cocacola.Descripcion = "Coca cola 3 litros";
            cocacola.ValorUnitario = 120;
            cocacola.Cantidad = 1;
            cocacola.Descuento = 20;
            cocacola.Importe = 120;

            //lists de impuestos para todos los conceptos 
            List<ComprobanteConceptoImpuestosTraslado> impuestos = new List<ComprobanteConceptoImpuestosTraslado>();

            //iva
            ComprobanteConceptoImpuestosTraslado iva = new ComprobanteConceptoImpuestosTraslado();
            iva.Base = 100;
            iva.TasaOCuota = 0.160000m;
            iva.TipoFactor = "Tasa";
            iva.Impuesto = "002";
            iva.Importe = 16;


            //ieps
            ComprobanteConceptoImpuestosTraslado ieps3 = new ComprobanteConceptoImpuestosTraslado();
            ieps3.Base = 100;
            ieps3.TasaOCuota = 0.030000m;
            ieps3.TipoFactor = "Tasa";
            ieps3.Impuesto = "003";
            ieps3.Importe = 3;


            impuestos.Add(iva);
            impuestos.Add(ieps3);
            cocacola.Impuestos = new ComprobanteConceptoImpuestos();
            cocacola.Impuestos.Traslados = impuestos.ToArray();


            conceptos.Add(cocacola);


            comprobante.Conceptos = conceptos.ToArray();

            //***************************Nodo impuesto a nivel comprobante***************************
            List<ComprobanteImpuestosTraslado> impuestosComprobante = new List<ComprobanteImpuestosTraslado>();
            ComprobanteImpuestos nodoImpuestoComprobante = new ComprobanteImpuestos();

            ComprobanteImpuestosTraslado imp1 = new ComprobanteImpuestosTraslado();
            ComprobanteImpuestosTraslado imp2 = new ComprobanteImpuestosTraslado();


            imp1.Importe = 16;
            imp1.Impuesto = "002";
            imp1.TipoFactor = "Tasa";
            imp1.TasaOCuota = 0.160000m;

            imp2.Importe = 3;
            imp2.Impuesto = "003";
            imp2.TipoFactor = "Tasa";
            imp2.TasaOCuota = 0.030000m;

            impuestosComprobante.Add(imp1);
            impuestosComprobante.Add(imp2);


            nodoImpuestoComprobante.Traslados = impuestosComprobante.ToArray();
            nodoImpuestoComprobante.TotalImpuestosTrasladados = 19;
            //agregamos impuesto a comprobante
            comprobante.Impuestos = nodoImpuestoComprobante;





            FacturaActual = "FACTURA_" + comprobante.Serie + comprobante.Folio + ".xml";
            CreateXML(comprobante);
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
            comprobante.Certificado = oSelloDigital.Certificado(RutaCer);
            comprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, RutaKey, ClavePrivada);

            CreateXML(comprobante);

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
