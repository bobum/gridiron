'use strict';

/*
 * Defining the Package
 */
var Module = require('meanio').Module;

var Player = new Module('player');

/*
 * All MEAN packages require registration
 * Dependency injection is used to define required modules
 */
Player.register(function(app, auth, database) {

  //We enable routing. By default the Package Object is passed to the routes
  Player.routes(app, auth, database);

  //We are adding a link to the main menu for all authenticated users
  Player.menus.add({
    title: 'player example page',
    link: 'player example page',
    roles: ['authenticated'],
    menu: 'main'
  });
  
  Player.aggregateAsset('css', 'player.css');

  /**
    //Uncomment to use. Requires meanio@0.3.7 or above
    // Save settings with callback
    // Use this for saving data from administration pages
    Player.settings({
        'someSetting': 'some value'
    }, function(err, settings) {
        //you now have the settings object
    });

    // Another save settings example this time with no callback
    // This writes over the last settings.
    Player.settings({
        'anotherSettings': 'some value'
    });

    // Get settings. Retrieves latest saved settigns
    Player.settings(function(err, settings) {
        //you now have the settings object
    });
    */

  return Player;
});
