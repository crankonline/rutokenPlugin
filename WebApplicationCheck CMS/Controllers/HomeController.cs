using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebApplicationCheck_CMS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Confirm(string cms)
        {
            try
            {
                ViewBag.Message = cms;
                SignedCms signedCms = new SignedCms();
                signedCms.Decode(Convert.FromBase64String(cms));
                signedCms.CheckSignature(false); //false - проверка подписи и сетрификата подписчика (по цепочке из cert Store)
                ContentInfo contentInfo = signedCms.ContentInfo;
                X509Certificate2Collection certificates = signedCms.Certificates;
                SignerInfoCollection signerInfo = signedCms.SignerInfos;
                CryptographicAttributeObjectCollection cryptographicAttributeObject = signerInfo[0].SignedAttributes;

                string timestamp = null;
                foreach (CryptographicAttributeObject cryptoAttribute in signerInfo[0].SignedAttributes)
                {
                    if (cryptoAttribute.Oid.Value == "1.2.840.113549.1.9.5")
                    {
                        Pkcs9AttributeObject rfcTimestampObj = new Pkcs9AttributeObject(cryptoAttribute.Values[0]);
                        Pkcs9SigningTime asnEncodedData = new Pkcs9SigningTime(cryptoAttribute.Values[0].RawData);
                        timestamp = asnEncodedData.SigningTime.ToString();
                        DateTime TDate = DateTime.ParseExact(timestamp, "dd.MM.yyyy H:mm:ss",
                                           System.Globalization.CultureInfo.InvariantCulture);
                        TDate = TDate.AddHours(6); //+6 часовой пояс
                        timestamp = TDate.ToString("dd.MM.yyyy H:mm:ss");
                    }
                }

                ViewBag.Content = Encoding.UTF8.GetString(contentInfo.Content);
                ViewBag.Certificate = certificates[0].Subject;
                ViewBag.TimeStamp = timestamp;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }
    }
}