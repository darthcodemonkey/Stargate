export interface Person {
  personId: number;
  name: string;
  currentRank: string | null;
  currentDutyTitle: string | null;
  careerStartDate: string | null;
  careerEndDate: string | null;
}

export interface CreatePerson {
  name: string;
}

export interface UpdatePerson {
  name: string;
}

