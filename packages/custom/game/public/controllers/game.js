(function () {
  'use strict';

  /* jshint -W098 */
  angular
    .module('mean.game')
    .controller('GameController', GameController);

  GameController.$inject = ['$scope', 'Global', 'Game'];

  function GameController($scope, Global, Game) {
    $scope.global = Global;
    $scope.package = {
      name: 'game'
    };
  }
})();