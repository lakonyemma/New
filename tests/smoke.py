"""Basic full-stack MVC integration smoke test against MySQL (disposable test database)."""
import http.cookiejar
import html
import re
import time
import urllib.error
import urllib.parse
import urllib.request

ROOT = "http://127.0.0.1:5080"
jar = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))


def request(path, data=None):
    body = None if data is None else urllib.parse.urlencode(data).encode()
    req = urllib.request.Request(ROOT + path, body, method="POST" if data is not None else "GET")
    try:
        with opener.open(req, timeout=8) as response:
            return response.status, response.geturl(), response.read().decode("utf8")
    except urllib.error.HTTPError as e:
        return e.code, e.url, e.read().decode("utf8", errors="replace")


def token(page):
    for tag in re.findall(r"<input\b[^>]+>", page, flags=re.I):
        if re.search(r'name="__RequestVerificationToken"', tag):
            m = re.search(r'value="([^"]+)"', tag)
            if m:
                return html.unescape(m.group(1))
    raise AssertionError("Missing anti-forgery token")


def check(condition, label):
    if not condition:
        raise AssertionError(label)
    print("PASS", label, flush=True)


for _ in range(90):
    try:
        status, _, _ = request("/health")
        if status == 200:
            break
    except Exception:
        time.sleep(0.5)
else:
    raise RuntimeError("MVC did not start; consult application logs")

status, _, login = request("/Account/Login")
check(status == 200 and "Welcome back" in login, "Razor login page renders")
status, url, page = request("/", {})
check("/Account/Login" in url, "Anonymous users cannot enter dashboard")

status, _, login = request("/Account/Login")
status, url, page = request("/Account/Login", {
    "__RequestVerificationToken": token(login),
    "UserName": "admin",
    "Password": "temporary_admin_test_password_123!",
})
check(status == 200 and "Good to have you here" in page, "Bootstrap admin signs in")

status, _, employee_form = request("/Employees/Create")
check(status == 200 and "Add someone" in employee_form, "Administrator can access employee creation")
status, url, page = request("/Employees/Create", {
    "__RequestVerificationToken": token(employee_form),
    "FullName": "Sarah Kato",
    "Email": "sarah@example.test",
    "Department": "Audit",
    "Skills": "Audit,Excel",
    "WeeklyHours": "40",
    "IsAvailable": "true",
    "UserName": "sarah",
    "InitialPassword": "temporary_employee_password_123!",
    "Role": "Employee",
})
check(status == 200 and "Sarah Kato" in page, "Admin creates an employee and account")

status, _, engagement_form = request("/Engagements/Create")
status, url, page = request("/Engagements/Create", {
    "__RequestVerificationToken": token(engagement_form),
    "ClientName": "ABC Limited",
    "Name": "Statutory Audit",
    "ServiceLine": "Audit",
    "Stage": "Planning",
    "DueDate": "2027-04-30",
    "Notes": "Initial client engagement",
})
check(status == 200 and "ABC Limited" in page, "Supervisor creates an engagement")
engagement_id = re.search(r'href="/Engagements/Edit/([0-9a-f-]{36})"', page)
check(engagement_id is not None, "Created engagement is linked from Razor view")

status, _, task_form = request("/Tasks/Create")
status, url, page = request("/Tasks/Create", {
    "__RequestVerificationToken": token(task_form),
    "Title": "Prepare audit plan",
    "Description": "Compile the planning working papers",
    "Department": "Audit",
    "RequiredSkills": "Audit,Excel",
    "Priority": "High",
    "EstimatedHours": "8",
    "DueDate": "2027-04-25",
    "EngagementId": engagement_id.group(1),
})
check(status == 200 and "Prepare audit plan" in page, "Supervisor creates a task with MySQL storage")
task_id = re.search(r"/Tasks/Details/([0-9a-f-]{36})", url)
check(task_id is not None, "New task redirects to detail screen")
eid = re.search(r'name="employeeId"[^>]*value="([0-9a-f-]{36})"', page)
check(eid is not None, "Allocation engine recommends skilled employee")
status, url, page = request("/Tasks/Assign/" + task_id.group(1), {
    "__RequestVerificationToken": token(page),
    "employeeId": eid.group(1),
})
check(status == 200 and "Sarah Kato" in page and "In Progress" in page, "Supervisor confirms intelligent task allocation")

status, _, page = request("/Account/Logout", {"__RequestVerificationToken": token(page)})
status, _, login = request("/Account/Login")
status, url, page = request("/Account/Login", {
    "__RequestVerificationToken": token(login),
    "UserName": "sarah",
    "Password": "temporary_employee_password_123!",
})
check(status == 200 and "Good to have you here" in page, "Employee signs in")

status, url, page = request("/Employees")
check("Access denied" in page, "Employee cannot access management-only team pages")
status, _, detail = request("/Tasks/Details/" + task_id.group(1))
check(status == 200 and "Update your progress" in detail, "Employee sees assigned task details")
status, _, progress = request("/Tasks/Progress/" + task_id.group(1), {
    "__RequestVerificationToken": token(detail),
    "progress": "100",
    "note": "Planning file ready for review",
})
check(status == 200 and "Submit for supervisor review" in progress, "Employee updates task progress")
status, _, submission = request("/Tasks/Submit/" + task_id.group(1), {
    "__RequestVerificationToken": token(progress),
})
check(status == 200 and "Awaiting Review" in submission, "Employee submits work for supervisor review")
print("All MVC / MySQL smoke checks passed.", flush=True)
