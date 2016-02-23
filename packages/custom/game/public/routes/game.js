(function () {
  'use strict';

  angular
    .module('mean.game')
    .config(game);

  game.$inject = ['$stateProvider'];

  function game($stateProvider) {
    $stateProvider.state('game example page', {
      url: '/game/example',
      templateUrl: 'game/views/index.html'
    });
  }

})();
