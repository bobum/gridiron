(function () {
  'use strict';

  angular
    .module('mean.game')
    .factory('Game', Game);

  Game.$inject = [];

  function Game() {
    return {
      name: 'game'
    };
  }
})();
