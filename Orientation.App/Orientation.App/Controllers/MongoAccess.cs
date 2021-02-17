using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Orientation.App.Models;
//using Newtonsoft.Json;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orientation.App.Controllers
{
    //[Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class MongoAccess : ControllerBase
    {
        //mongodb+srv://ninjadev:<password>@ninjamondemo.ddpof.mongodb.net/test

        //private readonly string mongoDbConnection = "mongodb+srv://ninjadev:ninjaP%40%24%24w0rd@ninjamonprojectscluster.ddpof.mongodb.net/trainings?retryWrites=true&w=majority";
        private readonly string mongoDbConnection = "mongodb+srv://ninjadev:ninjaP%40%24%24w0rd@ninjamondemo.ddpof.mongodb.net/trainings?retryWrites=true&w=majority";

        [Authorize]
        [HttpPost]
        [Route("api/mongodb/{collection}")]
        public IActionResult Post(string collection, [FromBody] object payload)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>(collection);
            var json = JsonSerializer.Serialize(payload);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            document.Add("dateStamp", DateTime.Now.ToString("yyyy-MM-dd"));
            mongoCollection.InsertOne(document);

            return Ok(true);
        }

        [Authorize]
        [HttpPut]
        [Route("api/mongodb/{collection}/{name}")]
        public IActionResult Put(string collection, string name,[FromBody] object payload)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>(collection);
            var json = JsonSerializer.Serialize(payload);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("name", name);
            var cat = document.GetValue("category");
            var filter2 = builder.Eq("category", cat.ToString());
            mongoCollection.ReplaceOne(filter1 & filter2, document);
            return Ok(true);
        }

        [Authorize]
        [HttpPost]
        [Route("api/mongodb/delete/{collection}/{name}")]
        public IActionResult Del(string collection, string name, [FromBody] object payload)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>(collection);
            var json = JsonSerializer.Serialize(payload);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("name", name);
            var cat = document.GetValue("category");
            var filter2 = builder.Eq("category", cat.ToString());
            mongoCollection.DeleteOne(filter1 & filter2);
            return Ok(true);
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/{collection}/{find}")]
        public IActionResult Get(string collection, string find)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>(collection);

            var record = new List<BsonDocument>();
            record = mongoCollection.Find(new BsonDocument()).ToList();

            return Ok(record.ConvertAll(BsonTypeMapper.MapToDotNetValue));
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/getbycategory/{collection}/{find}")]
        public IActionResult getbycategory(string collection, string find)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>(collection);
            var record = new List<BsonDocument>();
            var filter = Builders<BsonDocument>.Filter.Eq("category", find);
            var projection1 = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("files.dataURL");
            record = mongoCollection.Find(filter).Project(projection1).ToList();

            return Ok(record.ConvertAll(BsonTypeMapper.MapToDotNetValue));
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/getbycategorywithdeck/{category}")]
        public IActionResult getbycategorywithdeck(string category)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("orientation-decks");

            var record = new List<BsonDocument>();
            var filter = Builders<BsonDocument>.Filter.Eq("category", category);
            var projection1 = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("files.Size");
            record = mongoCollection.Find(filter).Project(projection1).Project(projection1).ToList();

            return Ok(record.ConvertAll(BsonTypeMapper.MapToDotNetValue));
        }

        [Authorize]
        [Route("api/mongodb/authorize")]
        public IActionResult Authorize()
        {
            return Ok("Hello world!");
        }

        [HttpGet]
        [Route("api/getcategories")]
        public IActionResult GetCategories()
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("categories");
            var result = mongoCollection.Find(new BsonDocument()).ToList();

            return Ok(result.ConvertAll(BsonTypeMapper.MapToDotNetValue));
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/getnewhires/{page}/{pageSize}/{search?}")]
        public IActionResult GetNewHires(int page, string pageSize, string search)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");

            var builder = Builders<BsonDocument>.Filter;
            // ^ - For Starts with
            // . - any character
            // * - for any number of previous "." 
            var filter = builder.Regex("name", $"^{search}.*") | builder.Regex("email", $".*{search}.*") | builder.Regex("empNo", $".*{search}.*") | builder.Regex("category", $".*{search}.*");

            var projection1 = Builders<BsonDocument>.Projection.Exclude("categoryWithDeck.files");

            IFindFluent<BsonDocument, BsonDocument> query = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = mongoCollection.Find(filter).Project(projection1);
            }
            else
            {
                query = mongoCollection.Find(new BsonDocument()).Project(projection1);
            }
            
            var total = query.CountDocuments();
            var pSize = 0;
            if(pageSize == "ALL")
            {
                pSize = 1000;
            }
            else
            {
                pSize = Convert.ToInt32(pageSize);
            }
            var skip = (page - 1) * pSize;
            var totalPage = (total + pSize - 1) / pSize;
            var result = query.Skip(skip).Limit(pSize).ToList();
            var convertedResult = result.ConvertAll(BsonTypeMapper.MapToDotNetValue);
            return Ok(new { total, result = convertedResult, totalPage });
        }

        [Authorize]
        [HttpPut]
        [Route("api/mongodb/updatenewhire/{empNo}")]
        public IActionResult Put(string empNo, [FromBody] object payload)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");
            var json = JsonSerializer.Serialize(payload);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            document.Add("dateStamp", DateTime.Now.ToString("yyyy-MM-dd"));
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("empNo", empNo);
            mongoCollection.ReplaceOne(filter1, document);
            return Ok(true);
        }

        [Authorize]
        [HttpDelete]
        [Route("api/mongodb/deletenewhire/{empNo}")]
        public IActionResult DeleteNewHire(string empNo)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("empNo", empNo);
            mongoCollection.DeleteOne(filter1);
            return Ok(true);
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/getorientation/{email}")]
        public IActionResult GetOrientation(string email)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("email", email);

            var result = mongoCollection.Find(filter1).ToList().ConvertAll(BsonTypeMapper.MapToDotNetValue).FirstOrDefault();

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/getorientationfiles/{category}/{name}")]
        public IActionResult GetOrientationFiles(string name, string category)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("orientation-decks");

            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("name", name);
            var filter2 = builder.Eq("category", category);

            var projection = Builders<BsonDocument>.Projection
                .Exclude("_id")
                .Exclude("category")
                .Exclude("description")
                .Exclude("files.base64")
                .Exclude("isComplete")
                .Exclude("completionTime");

            var result = mongoCollection.Find(filter1 & filter2).Project(projection).ToList().ConvertAll(BsonTypeMapper.MapToDotNetValue).FirstOrDefault();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/mongodb/completeorientation")]
        public IActionResult CompleteOrientation([FromBody] CompleteOrientationVm payload)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");

            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("email", payload.Email);
            var filter2 = builder.Eq("categoryWithDeck.name", payload.DeckName);
            
            var update = Builders<BsonDocument>.Update
                .Set("categoryWithDeck.$.isComplete", true).Set("categoryWithDeck.$.completionTime", DateTime.Now.ToString("MMM dd yyyy, h:mm tt"));

            mongoCollection.UpdateOne(filter1 & filter2, update);

            return Ok(true);
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/checkhire/{email}")]
        public IActionResult CheckHire(string email)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");

            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Eq("email", email);

            var projection = Builders<BsonDocument>.Projection
                .Exclude("_id")
                .Exclude("categoryWithDeck");

            var result = mongoCollection.Find(filter1).Project(projection).ToList().ConvertAll(BsonTypeMapper.MapToDotNetValue).FirstOrDefault();

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/dashboardtopdata")]
        public IActionResult DashboardTopData(string email)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("new-hires");

            var builder = Builders<BsonDocument>.Filter;
            var todayFilter = builder.Eq("dateStamp", DateTime.Now.ToString("yyyy-MM-dd"));
            var isCompleteFilter = builder.Eq("categoryWithDeck.isComplete", true);
            var inCompleteFilter = builder.Eq("categoryWithDeck.isComplete", false);
           

            var totalHires = mongoCollection.CountDocuments(new BsonDocument());
            var totalHiresToday = mongoCollection.CountDocuments(todayFilter);

            var totalCompleteOrientation = mongoCollection.CountDocuments(isCompleteFilter);

            var totalInCompleteOrientation = mongoCollection.CountDocuments(inCompleteFilter);
            return Ok(new { totalHires, totalHiresToday, totalCompleteOrientation, totalInCompleteOrientation });
        }

        [Authorize]
        [HttpGet]
        [Route("api/mongodb/openfile/{category}/{tabName}/{fileName}")]
        public IActionResult OpenFile(string category,string tabName,string fileName)
        {
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase("orientation");
            var mongoCollection = db.GetCollection<BsonDocument>("orientation-decks");
            var builder = Builders<BsonDocument>.Filter;
            var catFilter = builder.Eq("category",category);
            var fileFilter = builder.Eq("files.FileName", fileName);
            var tabFilter = builder.Eq("name", tabName);

            var projection = Builders<BsonDocument>.Projection
                .Exclude("_id")
                .Exclude("category")
                .Exclude("description")
                .Exclude("isComplete")
                .Exclude("dateStamp")
                .Exclude("files.Size");

            var result = mongoCollection.Find(catFilter & fileFilter & tabFilter).Project(projection).ToList().ConvertAll(BsonTypeMapper.MapToDotNetValue).FirstOrDefault();

            return Ok(result);
        }
    }
}
