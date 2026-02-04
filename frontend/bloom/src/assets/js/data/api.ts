import {type ApiError, fetchFromServer} from "./api-communication-abstractor.ts";
interface LoginResponse {
    token: string;
}

export async function login(email:string, password:string): Promise<string> {
  try {
    const response = await fetchFromServer<LoginResponse>(
        "auth/login",
        "POST",
        {
          email: email,
          password: password
        }
    );

      if ('token' in response === false) {
          throw response as ApiError;
      }

    localStorage.setItem("jwt", response.token);
    return response.token;
  } catch (error) {
    throw error;
  }
}

export async function register(email:string, password:string, height:number, weight:number, name:string, activeDays:number): Promise<string> {
  try {
    const response = await fetchFromServer<LoginResponse>(
        "auth/register",
        "POST",
        {
            email: email,
            password: password,
            height: height,
            weight: weight,
            name: name,
            activeDays: activeDays
        }
    );

    if ('token' in response === false) {
        throw response as ApiError;
    }

    localStorage.setItem("jwt", response.token);
    return response.token;
    } catch (error) {
    throw error;
  }
}