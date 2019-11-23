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
        Comprobante oComprobante;
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
            oComprobante = new Comprobante();
            oComprobante.Version = "3.3";
            oComprobante.Serie = "H";
            oComprobante.Folio = "1";
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            oComprobante.FormaPago = "99";
            oComprobante.NoCertificado = NoCertificado;
            oComprobante.SubTotal = 410.24m;
            oComprobante.Descuento = 0;
            oComprobante.Moneda = "MXN";
            oComprobante.Total = 430.97m;
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
            //Conceptos
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            oConcepto.Importe = 45.6m;
            oConcepto.ClaveProdServ = "01010101";
            oConcepto.Cantidad = 1;
            oConcepto.ClaveUnidad = "C81";
            oConcepto.Descripcion = "Un misil para la guerra";
            oConcepto.ValorUnitario = 45.6m;
            oConcepto.Descuento = 0;

            //impuesto trasladado SOLO IVA
            List<ComprobanteConceptoImpuestosTraslado> lstImpuestosTrasladados = new List<ComprobanteConceptoImpuestosTraslado>();
            ComprobanteConceptoImpuestosTraslado oImpuestoTrasladado = new ComprobanteConceptoImpuestosTraslado();
            oImpuestoTrasladado.Base = 45.6m;
            oImpuestoTrasladado.TasaOCuota = 0.160000m;
            oImpuestoTrasladado.TipoFactor = "Tasa";
            oImpuestoTrasladado.Impuesto = "002";
            oImpuestoTrasladado.Importe = 7.296m;
            lstImpuestosTrasladados.Add(oImpuestoTrasladado);

            oConcepto.Impuestos = new ComprobanteConceptoImpuestos();
            oConcepto.Impuestos.Traslados = lstImpuestosTrasladados.ToArray();

            lstConceptos.Add(oConcepto);

            //Concepto 2 IVA e IEPS
            ComprobanteConcepto oConcepto2 = new ComprobanteConcepto();
            oConcepto2.Importe = 55.98m;
            oConcepto2.ClaveProdServ = "01010101";
            oConcepto2.Cantidad = 1;
            oConcepto2.ClaveUnidad = "C81";
            oConcepto2.Descripcion = "concepto iva e ieps";
            oConcepto2.ValorUnitario = 55.98m;
            oConcepto2.Descuento = 0;

            List<ComprobanteConceptoImpuestosTraslado> lstImpuestosTrasladados2 = new List<ComprobanteConceptoImpuestosTraslado>();
            ComprobanteConceptoImpuestosTraslado oImpuestoTrasladado2 = new ComprobanteConceptoImpuestosTraslado();
            oImpuestoTrasladado2.Base = 55.98m;
            oImpuestoTrasladado2.TasaOCuota = 0.160000m;
            oImpuestoTrasladado2.TipoFactor = "Tasa";
            oImpuestoTrasladado2.Impuesto = "002";
            oImpuestoTrasladado2.Importe = 8.9568m;
            lstImpuestosTrasladados2.Add(oImpuestoTrasladado2);

            ComprobanteConceptoImpuestosTraslado oImpuestoTrasladadoIEPS = new ComprobanteConceptoImpuestosTraslado();
            oImpuestoTrasladadoIEPS.Base = 55.98m;
            oImpuestoTrasladadoIEPS.TasaOCuota = 0.080000m;//debe corresponder con un valor del catalogo, exactamente
            oImpuestoTrasladadoIEPS.TipoFactor = "Tasa";
            oImpuestoTrasladadoIEPS.Impuesto = "003";//clave ieps 003
            oImpuestoTrasladadoIEPS.Importe = 4.4784m;//el 25% de 9 es 2.25
            lstImpuestosTrasladados2.Add(oImpuestoTrasladadoIEPS);

            oConcepto2.Impuestos = new ComprobanteConceptoImpuestos();
            oConcepto2.Impuestos.Traslados = lstImpuestosTrasladados2.ToArray();

            lstConceptos.Add(oConcepto2);

            //concepto 3, iva exento 0----------------------------------------------------------
            ComprobanteConcepto oConcepto3 = new ComprobanteConcepto();
            oConcepto3.Importe = 308.66m;
            oConcepto3.ClaveProdServ = "01010101";
            oConcepto3.Cantidad = 1;
            oConcepto3.ClaveUnidad = "C81";
            oConcepto3.Descripcion = "concepto iva exento";
            oConcepto3.ValorUnitario = 308.66m;
            oConcepto3.Descuento = 0;

            //impuesto trasladado iva exento
            List<ComprobanteConceptoImpuestosTraslado> lstImpuestosTrasladados3 = new List<ComprobanteConceptoImpuestosTraslado>();
            ComprobanteConceptoImpuestosTraslado oImpuestoTrasladadoIVAExento = new ComprobanteConceptoImpuestosTraslado();
            oImpuestoTrasladadoIVAExento.Base = 308.66m;
            //  oImpuestoTrasladado.TasaOCuota = 0.160000m; exento no lleva tasa
            oImpuestoTrasladadoIVAExento.TipoFactor = "Exento";
            oImpuestoTrasladadoIVAExento.Impuesto = "002";//clave iva es 002
                                                          //oImpuestoTrasladado.Importe = 1.44m; exento no lleva importe
            lstImpuestosTrasladados3.Add(oImpuestoTrasladadoIVAExento);

            oConcepto3.Impuestos = new ComprobanteConceptoImpuestos();
            oConcepto3.Impuestos.Traslados = lstImpuestosTrasladados3.ToArray();


            lstConceptos.Add(oConcepto3);

            oComprobante.Conceptos = lstConceptos.ToArray();

            //NODO IMPUESTO*************************************************************************************
            List<ComprobanteImpuestosTraslado> lstImpuestoTRANSLADADOS = new List<ComprobanteImpuestosTraslado>();
            ComprobanteImpuestos oIMPUESTOS = new ComprobanteImpuestos();
            ComprobanteImpuestosTraslado oITIVA = new ComprobanteImpuestosTraslado();
            ComprobanteImpuestosTraslado oITIEPS = new ComprobanteImpuestosTraslado();
            oIMPUESTOS.TotalImpuestosTrasladados = 20.73m; //totales de impuestos trasladados

            //se agrupan los impuestos del mismo tipo, en este caso iva
            oITIVA.Importe = 16.2528m;
            oITIVA.Impuesto = "002";
            oITIVA.TipoFactor = "Tasa";
            oITIVA.TasaOCuota = 0.160000m;

            //ieps
            oITIEPS.Importe = 4.4784m;
            oITIEPS.Impuesto = "003";
            oITIEPS.TipoFactor = "Tasa";
            oITIEPS.TasaOCuota = 0.080000m;
          


            lstImpuestoTRANSLADADOS.Add(oITIVA);
            lstImpuestoTRANSLADADOS.Add(oITIEPS);
            oIMPUESTOS.Traslados = lstImpuestoTRANSLADADOS.ToArray();
            //agregamos impuesto a comprobante
            oComprobante.Impuestos = oIMPUESTOS;



            FacturaActual = "FACTURA_" + oComprobante.Serie + oComprobante.Folio + ".xml";

            //Crear Xml
            CreateXML(oComprobante);
            string cadenaOriginal = "";

            XslCompiledTransform transformador = new XslCompiledTransform(true);
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
            WSTimbrado.TimbradoClient oTimbrado = new WSTimbrado.TimbradoClient();

            byte[] bXML = File.ReadAllBytes(RutaComprobantes + FacturaActual);
            respuestaCFDI = oTimbrado.TimbrarTest("DGE131017IP1", "9616fb2b81e89673495f", bXML);

            if (respuestaCFDI.Documento == null)
            {
                MessageBox.Show(respuestaCFDI.Mensaje);
            }
            else
            {

                File.WriteAllBytes(RutaComprobantes + FacturaActual, respuestaCFDI.Documento);
                MessageBox.Show("Ok");

                Desearizar(FacturaActual);
                MessageBox.Show(oComprobante.TimbreFiscalDigital.UUID);

            }




        }


        private void Desearizar(string facturaActual)
        {

            //Paso 1 - xml timbrado lo pasamos a clase

            XmlSerializer oSerializer = new XmlSerializer(typeof(Comprobante));

            using (StreamReader reader = new StreamReader(RutaComprobantes + facturaActual))
            {
                //aqui deserializamos
                oComprobante = (Comprobante)oSerializer.Deserialize(reader);


                //complementos
                foreach (var oComplemento in oComprobante.Complemento)
                {
                    foreach (var oComplementoInterior in oComplemento.Any)
                    {
                        if (oComplementoInterior.Name.Contains("TimbreFiscalDigital"))
                        {

                            XmlSerializer oSerializerComplemento = new XmlSerializer(typeof(TimbreFiscalDigital));
                            using (var readerComplemento = new StringReader(oComplementoInterior.OuterXml))
                            {
                                oComprobante.TimbreFiscalDigital = (TimbreFiscalDigital)oSerializerComplemento.Deserialize(readerComplemento);
                            }

                        }
                    }
                }
            }

        }







        private void CreateXML(Comprobante oComprobante)
        {
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            xmlNameSpace.Add("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlSerializer serializer = new XmlSerializer(typeof(Comprobante));
            string sXml = "";
            using (var stringWriter = new StringWriterEncoding(Encoding.UTF8))
            {
                using (XmlWriter writter = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(writter, oComprobante, xmlNameSpace);
                    sXml = stringWriter.ToString();
                }
            }
            //guardamos el string en un archivo
            System.IO.File.WriteAllText(RutaComprobantes + FacturaActual, sXml);
        }




    }
}
