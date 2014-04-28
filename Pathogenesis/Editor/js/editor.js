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
  noStart = false;
  eSpawnerArr = [];
  eSpawnerID = 0;
  advMode = false;

  defSpawnProbObj = {
    normal: 50,
    big: 25,
    flying: 25
  }

  defSpawnLvlObj = {
    lvl1: 75,
    lvl2: 20,
    lvl3: 5
  }

  defSpawnPntObj = {
    Pos: {
      Vector2: {
        X: 0,
        Y: 0
      }
    }
  }

  defSpawnObj = {
    spawnProbs: defSpawnProbObj,
    levelProbs: defSpawnLvlObj,
    SpawnPoint: defSpawnPntObj
  }

  defaultLevelJson= '{"Level":{"Map":"","BackgroundTexture":"","Width":"","Height":"","Regions":"","_xmlns:xsi":"http://www.w3.org/2001/XMLSchema-instance","_xmlns:xsd":"http://www.w3.org/2001/XMLSchema"}}';
  defaultRegionJson= '{"Region":{"RegionSet":"","Center":"","SpawnPoints":""}}';

  Level = JSON.parse(defaultLevelJson);
  defRegion = JSON.parse(defaultRegionJson);
}

Regions = [];
regionSel = [];

function addRegion(){
  option = {
    id: regionSel.length,
    name: regionSel.length
  }
  regionSel.push(option);
  Regions.push(defRegion);
  Regions[regionSel.length-1].Region.SpawnPoints = [];
  $('#regsel').empty();
  $.each(regionSel, function(i, option) {
      $('#regsel').append($('<option/>').attr("value", option.id).text(option.name));
  });
  
  $( "#regsel" ).change(function() {
      for(var i=0; i<regionSel.length; i++){
        $('[reg'+i+']').removeClass("spawnArea");
      }
      $('[reg'+$('#regsel').val()+']').addClass("spawnArea");
  });

}

//BUTTON = SPAWN CLOSE
$("#espawn_close").on("click", function() {
  //Close the dialog
  $("#spawnEdit").hide();
  $(".tile").removeClass("spawnArea");
  $(".tile").removeClass("regCntr");
  advMode = false;
});

//Open Region Edit
$("#open_region_editor").on("click", function() {
  $("#RegionStuff").show();
  $("#NonRegionStuff").hide();
});

//Close Region Edit
$("#close_region_editor").on("click", function() {
  $("#RegionStuff").hide();
  $("#NonRegionStuff").show();
});

//BUTTON = REGION CREATE
$("#create_region").on("click", function() {
  addRegion();
  $("#regTools").show()
});


function modifyTile($cur){
  advMode = true;
  //Modify Action ofr eSpawner
  if ($cur.attr("class") == "tile eSpawner"){
    id = $cur.attr("eSpawnerID")
    console.log("selected");
    //Load relevant data
    //Set the sliders to their appropriate values
    if (eSpawnerArr[id] != null){
      for (var probType in eSpawnerArr[id].spawnProbs) {
        $('#prob_'+probType).val(eSpawnerArr[id].spawnProbs[probType]);
      }
      for (var probLvl in eSpawnerArr[id].levelProbs) {
        $('#prob_'+probLvl).val(eSpawnerArr[id].levelProbs[probLvl]);
      }
    }
    $("#spawnEdit").show();
    //Load the region areas

    //Code to color new regions - add to backend
  }
  //Saving Regions + Spawns
  $("#espawn_save").on("click", function() {
    //Save Probabilites
    for (var probType in eSpawnerArr[id].spawnProbs) {
      console.log(id);
      eSpawnerArr[id].spawnProbs[probType] = +$('#prob_'+probType).val();
    }
    for (var probLvl in eSpawnerArr[id].levelProbs) {
      eSpawnerArr[id].levelProbs[probLvl] = +$('#prob_'+probLvl).val();
    }
    //Saving Spawn Point in Region
    SpawnPoint = {
      SpawnPoint: eSpawnerArr[id].SpawnPoint
    }
    //Remove the spawnpoint from other regions if it is in other regions
    for (var i=0; i<regionSel.length; i++){
      for (var Spawnpoint in Regions[i].Region.SpawnPoints) {
        nX = SpawnPoint.Pos.X;
        nY = SpawnPoint.Pos.Y;
        oX = eSpawnerArr[id].SpawnPoint.Pos.X;
        oY = eSpawnerArr[id].SpawnPoint.Pos.Y;
        if (nX == oX && nY == oY){
          delete Spawnpoint;
        }
      }
    }
    //Save it
    Regions[$('#regsel').val()].Region.SpawnPoints.push(SpawnPoint);

    Regions[$('#regsel').val()].Region
    //Save Region Areas
    for(var i=0; i<regionSel.length; i++){
       Vector2arr = new Array();
      $('[reg'+i+']').each(function(){
       Vector2arr.push({
        X: $(this).attr("x"),
        Y: $(this).attr("y")
       })
       Vector2 = {
        Vector2: Vector2arr
       }
       Regions[i].Region.RegionSet = Vector2;
      })
    }
    //Save Region Center
    var data = {};
    $('[centerForReg="'+$('#regsel').val()+'"]').each(function(){
      //TODO: fix center
      data = {
        X: $(this).attr("x"),
        Y: $(this).attr("y")
      }
    });
    Regions[$('#regsel').val()].Region.Center = data;
  });
}

function drawing(){
  //Functions to draw on tiles
  function draw($cur){
    if(!advMode){

    if($cur.attr("class") == "tile eSpawner"){
      if(selectedTile != "modifier"){
        eSpawnerArr[$cur.attr("eSpawnerID")] = null;
      }
    }
    if(selectedTile == "modifier"){
      modifyTile($cur);
    }else{
      $cur.removeClass();
      $cur.addClass('tile');
      $cur.addClass(selectedTile);
      $cur.attr('type', selectedType);
      x = $cur.attr('x');
      y = $cur.attr('y');
      console.log('['+x+','+y+']');
      ArrayOfInt[y][x] = selectedType; 
    }
    //Add unique ID to enemy spawns
    if(selectedTile == "eSpawner"){
      $cur.attr('eSpawnerID', eSpawnerID);
      var newObject = jQuery.extend(true, {}, defSpawnObj);
      eSpawnerArr[eSpawnerID] = newObject;
      eSpawnerArr[eSpawnerID].SpawnPoint.Pos.Vector2.X = $cur.attr('x');
      eSpawnerArr[eSpawnerID].SpawnPoint.Pos.Vector2.Y = $cur.attr('y');
      eSpawnerID++;
    }
    //Methods for region stuff
    }else{
      if(selectedTile == "spawnArea"){
        $cur.addClass(selectedTile);
        $cur.attr("reg"+$('#regsel').val(), true);
      }else if(selectedTile == "regCntr"){
        //Make so only max 1 of these TODO-3
        $cur.addClass(selectedTile);
        $cur.attr("centerForReg",$('#regsel').val());
      }else if(selectedTile == "empty"){
        $cur.removeClass("spawnArea");
        $cur.removeClass("regCntr");
        $cur.removeAttr("reg"+$('#regsel').val());
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
    .click(function() {
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
        LevelXML = CreateXML();
        SaveXML(LevelXML, filename.val());

          $( this ).dialog( "close" );
      },
      Cancel: function() {
        $( this ).dialog( "close" );
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
          $('#output').empty();
          numTiles = lvlWidth.val() * lvlHeight.val();
          for(var y=0; y<lvlHeight.val(); y++){
            for(var x=0; x<lvlWidth.val(); x++){
              $("#container").append('<div class="tile empty" type="0" x="'+x+'" y="'+y+'"></div>');
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
Creating XML file
*/
function CreateXML(){
var x2js = new X2JS();



var WallTexture = {
  name: ""
}

for (var i=0; i<ArrayOfInt.length; i++){
  ArrayOfInt[i] = {
    int: ArrayOfInt[i]
  }
}

var tiles = {
  ArrayOfInt : ArrayOfInt
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

Level.Level.Regions = Regions;
Level.Level.Width = realWidth;
Level.Level.Height = realHeight;
Level.Level.Map = Map;
Level.Level.BackgroundTexture = BackgroundTexture;

var JSONlevel = JSON.stringify(Level);
var XMLlevel = x2js.json2xml_str($.parseJSON(JSONlevel));

//$("#output").text(XMLlevel);

return XMLlevel;
//I/O
};

function SaveXML(xml, filename){
  var blob = new Blob([xml], {type: "application/xhtml+xml;charset=ISO-8859-1"});
  saveAs(blob, filename);
};

$(function(view) {
  init();
  setup();
  $("#spawnEdit").hide();
  $("#RegionStuff").hide();
  $("#regTools").hide();
});

