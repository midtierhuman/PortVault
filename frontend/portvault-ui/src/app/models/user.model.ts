export enum Role {
  Admin = 'admin',
  User = 'user',
  Guest = 'guest',
}

export interface User {
  id: string;
  email: string;
  displayName?: string;
  roles: Role[];
  createdAt?: string;
  updatedAt?: string;
  isActive?: boolean;
}
