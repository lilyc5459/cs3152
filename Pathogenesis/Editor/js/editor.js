
function init() {
//Define Constants:
TILE_LENGTH = 40;
TILE_BORDER = 1;
TILE_REAL_LENGTH = 40;

ArrayOfInt = [];
realWidth = 0;
realHeight = 0;

defaultLevelJson= '{"Level":{"Map":"","BackgroundTexture":"","Width":"","Height":"","_xmlns:xsi":"http://www.w3.org/2001/XMLSchema-instance","_xmlns:xsd":"http://www.w3.org/2001/XMLSchema"}}';
Level = JSON.parse(defaultLevelJson);
}

$(function() {
  init();
  /*
  Creating, Saving, Loading Functions
  */
  	var lvlWidth = $( "#lvlWidth" ),
  	lvlHeight = $( "#lvlHeight" ),
  	allFields = $( [] ).add( lvlWidth ).add( lvlHeight );
    var selectedTile = 'empty';
    var selectedType = 0;


  /*
  Selecting Entity
  */
  $("#sidebar div").on("click", function() {
    $("#sidebar div").css("border", "grey solid");
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
      height: 300,
      width: 350,
      modal: true,
      buttons: {
        "Save a level": function() {
          allFields.removeClass( "ui-state-error" );
          //TODO: fix input
          filename = "map.xml";
          xml(filename);
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

         //Create defaulted map 
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

            //Functions to draw on tiles
            function draw($cur){
              $cur.removeClass();
              $cur.addClass('tile');
              $cur.addClass(selectedTile);
              $cur.attr('type', selectedType);
              x = $cur.attr('x');
              y = $cur.attr('y');
              console.log('['+x+','+y+']');
              ArrayOfInt[x][y] = selectedType;
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
            $("#container div").mousedown(function() {
              draw($(this));
            });

            $( this ).dialog( "close" );
          }
        },
        Cancel: function() {
          $( this ).dialog( "close" );
        }
      }
    });





  /*
  Creating XML file
  */
  function xml(filename){
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

  Level.Level.Width = realWidth;
  Level.Level.Height = realHeight;
  Level.Level.Map = Map;
  Level.Level.BackgroundTexture = BackgroundTexture;

  console.log(Level);

  var JSONlevel = JSON.stringify(Level);
  console.log(JSONlevel);
  var XMLlevel = x2js.json2xml_str($.parseJSON(JSONlevel));
  console.log(XMLlevel);

  $("#output").text(XMLlevel);

/*
  jsontest = '{"Level":{"Map":{"tiles":{"ArrayOfInt":[{"int":["1","1","0"]},{"int":["1","1","0"]},{"int":["1","1","0"]}]},"Width":"2020","Height":"1020","WallTexture":{"Name":""}},"BackgroundTexture":{"Name":""},"Width":"2020","Height":"1020","_xmlns:xsi":"http://www.w3.org/2001/XMLSchema-instance","_xmlns:xsd":"http://www.w3.org/2001/XMLSchema"}}';
  xmltest = '<?xml version="1.0" encoding="UTF-8"?><MyTest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><elf>23</elf><hello>Hello World</hello><testPoint><X>1</X><Y>1</Y></testPoint><tList /></MyTest>';
  var parsedJson = JSON.parse(jsontest);

  console.log(parsedJson);

  var myJSONText = JSON.stringify(parsedJson);

  console.log('myJSONText:'+myJSONText);


  var json2xml = x2js.json2xml_str($.parseJSON(myJSONText));

  console.log('json2xml -- xml: '+json2xml);

  var xml2json = x2js.xml_str2json(xmltest);

  console.log(xml2json);
*/

  };
});