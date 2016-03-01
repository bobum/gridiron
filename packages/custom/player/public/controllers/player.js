(function () {
  'use strict';

  /* jshint -W098 */
  angular
    .module('mean.player')
    .controller('PlayerController', PlayerController);

  PlayerController.$inject = ['$scope', 'Global', 'Player'];

  function PlayerController($scope, Global, Player) {
    $scope.global = Global;
    $scope.package = {
      name: 'player'
    };
  }
})();