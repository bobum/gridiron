// Enums matching C# DomainObjects enums

export enum Position {
  QB = 0,
  RB = 1,
  WR = 2,
  TE = 3,
  OL = 4,
  DL = 5,
  LB = 6,
  CB = 7,
  S = 8,
  K = 9,
  P = 10,
}

export enum Possession {
  Home = 0,
  Away = 1,
}

export enum Downs {
  First = 1,
  Second = 2,
  Third = 3,
  Fourth = 4,
}

export enum PlayType {
  Run = 0,
  Pass = 1,
  Kickoff = 2,
  Punt = 3,
  FieldGoal = 4,
}

export const PositionLabels: Record<Position, string> = {
  [Position.QB]: 'QB',
  [Position.RB]: 'RB',
  [Position.WR]: 'WR',
  [Position.TE]: 'TE',
  [Position.OL]: 'OL',
  [Position.DL]: 'DL',
  [Position.LB]: 'LB',
  [Position.CB]: 'CB',
  [Position.S]: 'S',
  [Position.K]: 'K',
  [Position.P]: 'P',
};
