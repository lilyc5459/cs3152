function init() {
  //Define Constants:
  TILE_LENGTH = 40;
  TILE_BORDER = 1;
  TILE_REAL_LENGTH = 40;

  ArrayOfInt = [];
  realWidth = 0;
  realHeight = 0;
  selectedTile = 'empty';
  selectedType = 0;
  eSpawnerArr = [];
  eSpawnerID = 0;
  advMode = false;

  //Spawn point information - spawn rates and level rates
  defSpawnProbObj = {
    tank: .75,
    flying: .25,
    lvl1: .75,
    lvl2: .20,
    lvl3: .05,
    rate: 10,
  }

  defSpawnerProbs = {
    spawnProbs: defSpawnProbObj,
    Id : -1
  }

  spawnRtsArr = [];


  defaultLevelJson = '{"Level":{"Map":"","BackgroundTexture":"","Width":"","Height":"","Regions":"","_xmlns:xsi":"http://www.w3.org/2001/XMLSchema-instance","_xmlns:xsd":"http://www.w3.org/2001/XMLSchema"}}';

  Level = JSON.parse(defaultLevelJson);
}

regionSel = [];

//BUTTON = SPAWN CLOSE
$("#espawn_close").on("click", function() {
  //Close the dialog
  $("#spawnEdit").hide();
});

//Open Region Edit
$("#open_region_editor").on("click", function() {
  advMode = true;
  $("#RegionStuff").show();
  $("#NonRegionStuff").hide();
  //Show region if it exists:
  if (regionSel.length>0){
    $('[reg'+$('#regsel').val()+']').addClass("spawnArea");
    $('[espawnerid'+$('#regsel').val()+'] > .spawnTxt').show();
    $('[centerForReg'+$('#regsel').val()+'] > .centerTxt').show();
  }
});

//Close Region Edit
$("#close_region_editor").on("click", function() {
  $("#RegionStuff").hide();
  $("#NonRegionStuff").show();
  $(".tile").removeClass("spawnArea");
  $(".tile").removeClass("regCntr");
  $(".spawnTxt").hide();
  $(".centerTxt").hide();
  advMode = false;
});

//BUTTON = REGION CREATE
$("#create_region").on("click", function() {
  addRegion(regionSel.length);
});

//BUTTON = Add Row
$(".addRow").on("click", function() {
  addRow();
});

//BUTTON = Add Col
$(".addCol").on("click", function() {
  addCol();
});

//BUTTON = Add Borders
$(".addBorders").on("click", function() {
  addBorders();
});

function addBorders(){
  var blockWidth = (realWidth/TILE_REAL_LENGTH)-1;
  var blockHeight = (realHeight/TILE_REAL_LENGTH)-1;
  for (var x = 0; x<=blockWidth; x++){
    ArrayOfInt[0][x] = 1;
    ArrayOfInt[blockHeight][x] = 1;
  }
  for (var y = 0; y<=blockHeight; y++){
    ArrayOfInt[y][0] = 1;
    ArrayOfInt[y][blockWidth] = 1;
  }
  $('.tile[x="0"]').removeClass().addClass("tile").addClass("wall").attr("type",1);
  $('.tile[y="0"]').removeClass().addClass("tile").addClass("wall").attr("type",1);
  $('.tile[x="'+blockWidth+'"]').removeClass().addClass("tile").addClass("wall").attr("type",1);
  $('.tile[y="'+blockHeight+'"]').removeClass().addClass("tile").addClass("wall").attr("type",1);
}

function addRow(){
  //Do Math for HTML elements
  var blockWidth = realWidth/TILE_REAL_LENGTH;
  var blockHeight = (realHeight/TILE_REAL_LENGTH)+1;
  finHeight = blockHeight * TILE_BORDER + blockHeight * TILE_LENGTH;
  
  for (var x=0; x<blockWidth; x++){
    y = blockHeight-1;
    $("#container").append('<div class="tile empty" type="0" x="'+x+'" y="'+y+'"><div class="spawnTxt">Spawn</div></br><div class="centerTxt">Center</div></div>');
  }

  realHeight = blockHeight * TILE_REAL_LENGTH;

  //Set HMTL Elements
  $(".tile").width(TILE_LENGTH);
  $(".tile").height(TILE_LENGTH);
  $('#container').height(finHeight);
}

function addCol(){

  //Do Math for HTML elements
  var blockWidth = (realWidth/TILE_REAL_LENGTH)+1;
  var blockHeight = realHeight/TILE_REAL_LENGTH;
  finHeight = blockHeight * TILE_BORDER + blockHeight * TILE_LENGTH;
  finWidth = blockWidth * TILE_BORDER + blockWidth * TILE_LENGTH;

  for (var y=0; y<blockHeight; y++){
    x = blockWidth-1;
    oldX = blockWidth-2;
    $('.tile[x="'+oldX+'"][y="'+y+'"]').after('<div class="tile empty" type="0" x="'+x+'" y="'+y+'"><div class="spawnTxt">Spawn</div></br><div class="centerTxt">Center</div></div>');
  }

  realWidth = blockWidth * TILE_REAL_LENGTH;

  //Set HMTL Elements
  $(".tile").width(TILE_LENGTH);
  $(".tile").height(TILE_LENGTH);
  $('#container').width(finWidth);
}


function addRegion(regId){
  option = {
    id: regId,
    name: regId
  }
  regionSel.push(option);
  $('#regsel').empty();
  $.each(regionSel, function(i, option) {
      $('#regsel').append($('<option/>').attr("value", option.id).text(option.name));
  });
  $( "#regsel" ).change(function() {
      for(var i=0; i<regionSel.length; i++){
        $('[reg'+i+']').removeClass("spawnArea");
        $('[espawnerid'+i+'] > .spawnTxt').hide();
        $('[centerForReg'+i+'] > .centerTxt').hide();
      }
      $('[reg'+$('#regsel').val()+']').addClass("spawnArea");
      $('[espawnerid'+$('#regsel').val()+'] > .spawnTxt').show();
      $('[centerForReg'+$('#regsel').val()+'] > .centerTxt').show();
      //show relevant
  });
  $("#regTools").show()
}

function modifyTile($cur){
  id = $cur.attr('espawnerid'+$('#regsel').val());
  //Load relevant data
  //Set the sliders to their appropriate values
  if (spawnRtsArr[id] != null){
    for (var probType in spawnRtsArr[id].spawnProbs) {
      $('#prob_'+probType).val(spawnRtsArr[id].spawnProbs[probType]);
    }
  }
  $("#spawnEdit").show();
  //Load the region areas

    //Code to color new regions - add to backend
  //Saving Regions + Spawns
  $("#espawn_save").on("click", function() {
    //Save Probabilites
    for (var probType in spawnRtsArr[id].spawnProbs) {
      spawnRtsArr[id].spawnProbs[probType] = +$('#prob_'+probType).val();
    }
    $("#spawnEdit").hide();
  })
}

function drawing(){
  //Functions to draw on tiles
  function draw($cur){
    if(!advMode){
      $cur.removeClass();
      $cur.addClass('tile');
      $cur.addClass(selectedTile);
      $cur.attr('type', selectedType);
      x = $cur.attr('x');
      y = $cur.attr('y');
      ArrayOfInt[y][x] = selectedType; 
    //Methods for region stuff!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
    }else{
      if(selectedTile == "empty"){
        //remove spawner
        //-remove object TODO: Do I need to really set this to null?
        eSpawnerIDtoRemove = $cur.attr('eSpawnerID'+$('#regsel').val());
        if (eSpawnerIDtoRemove >= 0){
          spawnRtsArr[eSpawnerIDtoRemove] = null;
        }
        //-remove html
        $cur.removeAttr('eSpawnerID'+$('#regsel').val());
        //remove center
        $cur.removeAttr('centerForReg'+$('#regsel').val());
        //remove area
        $cur.removeAttr("reg"+$('#regsel').val());

        //Remove Visulizations
        $cur.removeClass("spawnArea");
        $cur.children(".spawnTxt").hide();
        $cur.children(".centerTxt").hide();
      }
      else if(selectedTile == "modifier"){
        if ($cur.attr('eSpawnerID'+$('#regsel').val()) >= 0) {
          modifyTile($cur);
        }
      }
      else if(selectedTile == "spawnArea"){
        $cur.addClass(selectedTile);
        $cur.attr("reg"+$('#regsel').val(), true);
      }
      else if ($cur.attr("class").indexOf("spawnArea") !== -1){
        //Add unique ID to enemy spawns
        if(selectedTile == "eSpawner"){
          //Add spawner class and show text
          $cur.attr('eSpawnerID'+$('#regsel').val(), eSpawnerID);
          $cur.children(".spawnTxt").show();
          //Javascript spaner info creation
          var newDefSpwnRates = jQuery.extend(true, {}, defSpawnerProbs);
          newDefSpwnRates.Id = eSpawnerID;
          spawnRtsArr[eSpawnerID] = newDefSpwnRates;
          eSpawnerID++;
        }
        else if(selectedTile == "regCntr"){
          $('[centerForReg'+$('#regsel').val()+'] > .centerTxt').hide();
          $('[centerForReg'+$('#regsel').val()+']').removeAttr('centerforreg'+$('#regsel').val());
          $cur.attr("centerForReg"+$('#regsel').val(), true);
          $cur.children(".centerTxt").show();
        }
      }
    }
  }

  //Drawing Stuff
  $(document).mousedown(function() {
      $("#container div").bind('mouseover',function(){
          draw($(this));
      });
  })
  .mouseup(function() {
    $("#container div").unbind('mouseover');
  });
  $(".tile").mousedown(function() {
    draw($(this));
  });
}

function setup(){
  /*
  Creating, Saving, Loading Functions
  */
    var lvlWidth = $( "#lvlWidth" ),
    lvlHeight = $( "#lvlHeight" ),
    filename = $("#filename"),
    allFields = $( [] ).add( lvlWidth ).add( lvlHeight ).add( filename );



  /*
  Selecting Entity
  */
  $(".entity").on("click", function() {
    $(".entity").css("border", "grey solid");
    $(this).css("border", "red dotted");
    selectedTile = $(this).attr('id');
    selectedType = $(this).attr('type');
  })

    $( "#new" )
      .button()
      .click(function() {
        $( "#new-form" ).dialog( "open" );
      });

    $( "#save" )
      .button()
      .click(function(e) {
        $( "#save-form" ).dialog( "open" );
      });

    $( "#save-form" ).dialog({
      autoOpen: false,
      height: 240,
      width: 350,
      modal: true,
      buttons: {
        "Save a level": function() {
          allFields.removeClass( "ui-state-error" );
          LevelXML = CreateLevelXML();
          SaveXML(LevelXML, filename.val());
          $( this ).dialog( "close" );
        },
        Cancel: function() {
          $( this ).dialog( "close" );
        },
        "Save Spanwers": function() {
          allFields.removeClass( "ui-state-error" );
          LevelXML = CreateLevelXML();
          SpawnRatesXML = CreateSpawnRatesXML();
          //SaveXML(LevelXML, filename.val());
          SaveXML(SpawnRatesXML, '[SPAWNRATE]'+filename.val());
        }
      }
    });

    $( "#new-form" ).dialog({
      autoOpen: false,
      height: 300,
      width: 350,
      modal: true,
      buttons: {
        "Create a level": function() {
          var bValid = true;
          allFields.removeClass( "ui-state-error" );

          bValid = bValid && $.isNumeric(lvlWidth.val());
          bValid = bValid && $.isNumeric(lvlHeight.val());

          //Create map 
          if ( bValid ) {
            finWidth = lvlWidth.val() * TILE_BORDER + lvlWidth.val() * TILE_LENGTH;
            finHeight = lvlHeight.val() * TILE_BORDER + lvlHeight.val() * TILE_LENGTH;
            
            realWidth = lvlWidth.val() * TILE_REAL_LENGTH;
            realHeight = lvlHeight.val() * TILE_REAL_LENGTH;

            //Create empty inner arrays
            intArr =  new Array(lvlWidth.val());
            for(var k=0; k<lvlWidth.val(); k++){
              intArr[k] = 0;
            }

            ArrayOfInt = new Array(lvlHeight.val());
            //Create out array to hold inner arrays
            for(var j=0; j<lvlHeight.val(); j++){
              int = intArr.slice(0);
              ArrayOfInt[j] = int;
            }


            $('#container').width(finWidth);
            $('#container').height(finHeight);

            $('#container').empty();

            for(var y=0; y<lvlHeight.val(); y++){
              for(var x=0; x<lvlWidth.val(); x++){
                $("#container").append('<div class="tile empty" type="0" x="'+x+'" y="'+y+'"><div class="spawnTxt">Spawn</div></br><div class="centerTxt">Center</div></div>');
              }
            }

            $(".tile").width(TILE_LENGTH);
            $(".tile").height(TILE_LENGTH);

            drawing();

            $( this ).dialog( "close" );
          }
        },
        Cancel: function() {
          $( this ).dialog( "close" );
        }
      }
    });
}


/*
Creating Spwn XML file
*/
function CreateSpawnRatesXML(){
  var x2js = new X2JS();
  var objSpawnRatesArr = new Array();
  for(var i=0; i<regionSel.length; i++){
    $('[eSpawnerID'+i+']').each(function(){
       index = $(this).attr('eSpawnerID'+i);
       if (spawnRtsArr[index] != null){
        objSpawnRatesArr.push(spawnRtsArr[index]);
      }
    })
  }
  if (objSpawnRatesArr != null){
    OBJspawnRates = {
      SpawnInfoArray : {
        SpawnInfo : objSpawnRatesArr,
      }
    }
  }else{
    return 0;
  }
  var JSONSpawnRates = JSON.stringify(OBJspawnRates);
  var XMLSpawnRates = x2js.json2xml_str($.parseJSON(JSONSpawnRates));

  return XMLSpawnRates;
}

/*
Creating XML file
*/
function CreateLevelXML(){
var x2js = new X2JS();

var WallTexture = {
  name: ""
}

var ArrayOfIntPub = new Array();
ArrayOfIntPub = $.extend(true, [], ArrayOfInt);

for (var i=0; i<ArrayOfIntPub.length; i++){
  ArrayOfIntPub[i] = {
    int: ArrayOfIntPub[i]
  }
}

var tiles = {
  ArrayOfInt : ArrayOfIntPub
}

var Map = {
  tiles: tiles,
  Width: realWidth,
  Height: realHeight,
  WallTexture: WallTexture
}

var BackgroundTexture = {
  name: ""
}

var defSpawnPntObj = {
  Pos : {
    Vector2 : {
      X: -1,
      Y: -1
    }
  },
  Id : -1
}

var defSpawnObj = {
  SpawnPoint: defSpawnPntObj
}

Regions = [];
//---SaveRegion stuff---
//Save Region Areas
defaultRegionJson = '{"Region":{"RegionSet":"","Center":"","SpawnPoints":""}}';
for(var i=0; i<regionSel.length; i++){
  defRegionObj = JSON.parse(defaultRegionJson);
  Regions[i] = defRegionObj;
  Regions[i].Region.SpawnPoints = [];
  Vector2arr = new Array();
  //loop through each relevant area block
  $('[reg'+i+']').each(function(){
    Vector2arr.push({
      X: $(this).attr("x"),
      Y: $(this).attr("y")
    })
    Vector2 = {
      Vector2: Vector2arr
    };
    Regions[i].Region.RegionSet = Vector2;
  })

  //loop through each relevant espawner
  //TODO: Fix regions to going into same array
  var SpawnPointArr = new Array();
  $('[eSpawnerID'+i+']').each(function(indexesp){
    var index  = $(this).attr('eSpawnerID'+i);
    var newDefSpawnObject = jQuery.extend(true, {}, defSpawnObj);
    eSpawnerArr[index] = newDefSpawnObject;
    eSpawnerArr[index].SpawnPoint.Pos.Vector2.X = $(this).attr('x');
    eSpawnerArr[index].SpawnPoint.Pos.Vector2.Y = $(this).attr('y');
    eSpawnerArr[index].SpawnPoint.Id = index;
    //If object not empty, push
    if(!jQuery.isEmptyObject(eSpawnerArr[index].SpawnPoint)){
      SpawnPointArr.push(eSpawnerArr[index].SpawnPoint);
    }
  })
  finSpawnPoint = {
    SpawnPoint: SpawnPointArr
  };
  Regions[i].Region.SpawnPoints = finSpawnPoint;

  //loop through each relevant center
  var centerPts = {};
  $('[centerForReg'+i+']').each(function(){
    centerPts = {
      X: $(this).attr("x"),
      Y: $(this).attr("y")
    };
  })
  Regions[i].Region.Center = centerPts;
}

regionArr2 = [];
for(var i=0; i<Regions.length; i++){
  console.log(Regions[i]);
  if (Regions[i].Region.RegionSet != null && Regions[i].Region.RegionSet != ""){
    regionArr2.push(  
                      { RegionSet : Regions[i].Region.RegionSet,
                        Center : Regions[i].Region.Center,
                        SpawnPoints : Regions[i].Region.SpawnPoints
                      }
                    );
  }
}

finRegions = {
  Region: regionArr2
};


//Build Level Object
Level.Level.Regions = [];
Level.Level.Regions = finRegions;
Level.Level.Width = realWidth;
Level.Level.Height = realHeight;
Level.Level.Map = Map;
Level.Level.BackgroundTexture = BackgroundTexture;

var JSONlevel = JSON.stringify(Level);
var XMLlevel = x2js.json2xml_str($.parseJSON(JSONlevel));

//$("#output").text(XMLlevel);

return XMLlevel;
};

function SaveXML(xml, filename){
  var blob = new Blob([xml], {type: "application/xhtml+xml;charset=ISO-8859-1"});
  saveAs(blob, filename);
};


function loadMap (file) {
  var reader = new FileReader();
  reader.onload = function() {
    buildLevel(this.result);
  }
  reader.readAsText(file)
}

function loadRates (file) {
  var reader = new FileReader();
  reader.onload = function() {
    buildRates(this.result);
  }
  reader.readAsText(file)
}

function buildRates(inputTxt){
  var x2js = new X2JS();
  var rateObj = x2js.xml_str2json( inputTxt );

  var MaxId = 0;
  for (var i=0; i<rateObj.SpawnInfoArray.SpawnInfo.length; i++){
    spawnId = rateObj.SpawnInfoArray.SpawnInfo[i].Id;
    if (spawnId > MaxId){
      MaxId = spawnId;
    }
    var newDefSpwnRates = jQuery.extend(true, {}, defSpawnerProbs);
    newDefSpwnRates.Id = spawnId;
    newDefSpwnRates.spawnProbs = rateObj.SpawnInfoArray.SpawnInfo[i].spawnProbs;
    spawnRtsArr[spawnId] = newDefSpwnRates;
  }
  eSpawnerID = MaxId + 1;
  $("#loadRates").hide();
}

function buildLevel(inputTxt){
  //Convert input into an object
  var x2js = new X2JS();
  var levelObj = x2js.xml_str2json( inputTxt );

  //Clear the container
  $('#container').empty();
  //Load Array of integers
  for (var y = 0; y<levelObj.Level.Map.tiles.ArrayOfInt.length; y++){
    var innerArr = new Array();
    for (var x = 0; x<levelObj.Level.Map.tiles.ArrayOfInt[y].int.length; x++){
      typeVal = +levelObj.Level.Map.tiles.ArrayOfInt[y].int[x];
      innerArr.push(typeVal);
      var className;
      if(typeVal == 0){
        className = "empty";
      }
      else if (typeVal == 1){
        className = "wall";
      }
      else if (typeVal == 8){
        className = "pStart";
      }
      else if (typeVal == 7){
        className = "itemHolder";
      }
      else if (typeVal == 9){
        className = "goalBoss";
      }
      $("#container").append('<div class="tile '+className+'" type="'+typeVal+'" x="'+x+'" y="'+y+'"><div class="spawnTxt">Spawn</div></br><div class="centerTxt">Center</div></div>');
    }
    ArrayOfInt[y]=innerArr;
  }

  //Loop through regions and add area,spawners,centers
  var curReg = 0;
  console.log(levelObj.Level);
  for (var j=0; j<levelObj.Level.Regions.Region.length; j++) {
    //Select tiles inside region and assign them the reg attribute
    if (levelObj.Level.Regions.Region[j].RegionSet != "") {
      console.log(curReg);
      addRegion(curReg);
      console.log(levelObj.Level.Regions);
      for (var i=0; i<levelObj.Level.Regions.Region[j].RegionSet.Vector2.length; i++){
        xCord = +levelObj.Level.Regions.Region[j].RegionSet.Vector2[i].X;
        yCord = +levelObj.Level.Regions.Region[j].RegionSet.Vector2[i].Y;
        $('.tile[x="'+xCord+'"][y="'+yCord+'"]').attr('reg'+curReg, true);
      }
      centXcord = +levelObj.Level.Regions.Region[j].Center.X;
      centYcord = +levelObj.Level.Regions.Region[j].Center.Y;
      $('.tile[x="'+centXcord+'"][y="'+centYcord+'"]').attr('centerforreg'+curReg, true);
      //Loop through spawners  -- other thing
      console.log("j :"+j);
      splength = levelObj.Level.Regions.Region[j].SpawnPoints.SpawnPoint.length;
      console.log(splength);
      for (var k=0; k<splength; k++){
        spawnId = +levelObj.Level.Regions.Region[j].SpawnPoints.SpawnPoint[k].Id;
        SpawnxCord = +levelObj.Level.Regions.Region[j].SpawnPoints.SpawnPoint[k].Pos.Vector2.X;
        SpawnyCord = +levelObj.Level.Regions.Region[j].SpawnPoints.SpawnPoint[k].Pos.Vector2.Y;
        $('.tile[x="'+SpawnxCord+'"][y="'+SpawnyCord+'"]').attr('espawnerid'+curReg, spawnId);
      }
    }
  curReg++;
  }

  //Load width and height
  realWidth = levelObj.Level.Width;
  realHeight = levelObj.Level.Height;

  //Do Math for HTML elements
  var blockWidth = realWidth/TILE_REAL_LENGTH;
  var blockHeight = realHeight/TILE_REAL_LENGTH;
  finWidth = blockWidth * TILE_BORDER + blockWidth * TILE_LENGTH;
  finHeight = blockHeight * TILE_BORDER + blockHeight * TILE_LENGTH;
  
  //Set HMTL Elements
  $(".tile").width(TILE_LENGTH);
  $(".tile").height(TILE_LENGTH);
  $('#container').width(finWidth);
  $('#container').height(finHeight);

  //Get ready to load rates...
  $('#loadRates').show();

  drawing();
}

$('#file').change(function(){
  loadMap($(this.files)[0]);
})

$('#rates').change(function(){
  loadRates($(this.files)[0]);
})

$(function(view) {
  init();
  setup();
  $("#spawnEdit").hide();
  $("#RegionStuff").hide();
  $("#regTools").hide();
});