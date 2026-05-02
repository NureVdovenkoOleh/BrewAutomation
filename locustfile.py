from locust import HttpUser, task, between

class BrewUser(HttpUser):
    wait_time = between(1, 3) 
    token = ""

    def on_start(self):
        response = self.client.post("/api/Auth/login", json={
            "email": "testuser@example.com",
            "password": "Password123!"
        })
        if response.status_code == 200:
            self.token = response.json().get("token")

    @task(2)
    def get_recipes(self):
        # Завдання 1: Отримати список рецептів (виконується частіше)
        headers = {"Authorization": f"Bearer {self.token}"}
        self.client.get("/api/Recipe", headers=headers)

    @task(1)
    def get_history(self):
        # Завдання 2: Отримати історію варок
        headers = {"Authorization": f"Bearer {self.token}"}
        self.client.get("/api/BrewSession", headers=headers)