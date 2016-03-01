(function () {
  'use strict';

  angular
    .module('mean.player')
    .factory('Player', Player);

  Player.$inject = [];

  function Player() {
    return {
      name: 'player'
    };
  }
})();
