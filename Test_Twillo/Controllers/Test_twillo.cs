using Microsoft.AspNetCore.Mvc;
using Test_Twillo.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.AspNetCore.Cors;

namespace Test_Twillo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [EnableCors("AnotherPolicy")]
    public class Test_twillo : ControllerBase
    {
        /*
         *  ---- Metodo Create---
         *
         *      Crea el mensaje solo con puro texto[head-body], el bhead y body no pueden ir nulos 
         *      si llegan nulos deben replazarce por un texto de defautl
         *
        */
        [HttpPost("/Create")]
        public ActionResult Create([FromBody] ObjectData data)
        {
            Console.WriteLine(data.Titulo);
            Console.WriteLine(data.Message);
            Console.WriteLine(data.NumberTo);
            Console.WriteLine(data.File);
            Console.WriteLine(data.NameFile);
            //moverlos estas variables al appsettings
            string accountSid = "";//ceunta de twilio
            string authToken = "";//token de twilio

            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions( new PhoneNumber($"whatsapp:+521{data.NumberTo}") );
            messageOptions.From = new PhoneNumber("whatsapp:+14155238886");
            messageOptions.Body = $"{data.Titulo}\n{data.Message}";
            
            var message = MessageResource.Create(messageOptions);

            return Ok(message.Sid);
        }
        /*
         *  ---- Metodo Create---
         *
         *      Crea el mensaje con archivo adjunto por medio de URL y 
         *      el texto[head-body], el body puede ir nulo
         *
        */
        [HttpPost("/Send")]
        public ActionResult Send([FromBody] ObjectData data)
        {
            Console.WriteLine(data.Titulo);
            Console.WriteLine(data.Message);
            Console.WriteLine(data.NumberTo);
            Console.WriteLine(data.File);
            Console.WriteLine(data.NameFile);
            string accountSid = "";//ceunta de twilio
            string authToken = "";//token de twilio

            TwilioClient.Init(accountSid, authToken);

            var mediaUrl = new[] {
            //new Uri("https://images.unsplash.com/photo-1545093149-618ce3bcf49d?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=668&q=80")
            new Uri(data.File)
            }.ToList();

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:+521{data.NumberTo}"));
            messageOptions.From = new PhoneNumber("whatsapp:+14155238886");
            messageOptions.Body = $"{data.Titulo}\n{data.Message}";
            messageOptions.MediaUrl = mediaUrl;


            var message = MessageResource.Create(messageOptions);

            return Ok(message.Sid);
        }

        /*
         *  ---Received--
         * 
         *  Metodo recibe la imagen por form , la gurda en el servidor y regrea el nombre del archivo,
         *  para concatenarce con la url en el frond - end
         *  
         * 
         * 
         *  modificar para remplazar el local host por un servidor de prueba web000
         */
        [HttpPost("/Received")]
        public ActionResult Received([FromForm] List<IFormFile> files)
        {
            bool flag = false;
            List<string> filesNames = new List<string>();//lista de archivos para guardar en el servdior, puede ser  1 o > 1
            try
            {
                string path = @"/file_Temp/";
                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                foreach (var file in files)
                {
                    using (var stream = System.IO.File.Create(path+file.FileName))
                    {
                        file.CopyToAsync(stream);
                        filesNames.Add(file.FileName);
                    }
                }
                flag = true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return flag == true ? Ok(filesNames): BadRequest((400,"Error"));
        }


        /*
                --- Received/{file}--
                
        
                se envia el nombre para bsuacrlo en el localhost junto con el content type 
                y regresa el file completo para descargarlo.


                NOTA: este metodo regresa como metodo de descarga el archivo completo generando un venta de desacarga,
                lo que el api de twilio no admite y genera error, cambiar metodo
         */

        [HttpGet("/Received/{file}")]
        public ActionResult Recover( string file)
        {
            string content_type = "";
            string extencion = file.Substring(file.LastIndexOf('.') + 1);
            switch (extencion)// verificamos la extencion para que sea una extencion valida y asi regresar el contentype correcto
            {
                case "jpg":
                    content_type = "image/jpeg";
                    break;
                case "png":
                    content_type = "image/png";
                    break;
                case "xml":
                    content_type = "text/xml";
                    break;
                case "pdf":
                    content_type = "application/pdf";
                    break;
                case "zip":
                    content_type = "application/zip";
                    break;
                default:
                    content_type = "application/octet-stream";
                    break;
            }
            return PhysicalFile(@"/file_Temp/"+file, content_type, file);//retorna el archivo talcual como file de descrga en el navegador.
        }
    }
}
